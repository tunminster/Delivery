using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Collections;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Validation;
using Delivery.Azure.Library.WebApi.Extensions;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.WebApi.OData
{
    public class QueryableOperationAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var request = context.HttpContext.Request;
			if (request == null)
			{
				throw new AccessViolationException("Service is not for public use");
			}

			var queryContract = new QueryableContract();

			var queryCollection = request.Query;

			try
			{
				queryCollection.Keys.ForEach(key =>
				{
					switch (key.ToLowerInvariant())
					{
						case "$skip":
							if (!uint.TryParse(queryCollection[key].ToString(), out var skipCount))
							{
								throw new NotSupportedException($"{nameof(key)} must be an unsigned integer.");
							}

							queryContract.Skip = skipCount;

							break;
						case "$top":
							if (!uint.TryParse(queryCollection[key].ToString(), out var takeCount))
							{
								throw new NotSupportedException($"{nameof(key)} must be an unsigned integer.");
							}

							queryContract.Top = takeCount;

							break;
						case "$orderby":
							queryContract.Sort = queryCollection[key].ToString();
							break;
						case "$filter":
							queryContract.Filter = queryCollection[key].ToString();
							break;
						case "$culture":
							var value = queryCollection[key].ToString();
							if (!string.IsNullOrWhiteSpace(value))
							{
								queryContract.SetCultureInfo(new CultureInfo(value));
							}

							break;
						default:
							throw new NotSupportedException($"{nameof(key)} is not a supported type.");
					}
				});
			}
			catch (NotSupportedException notSupportedException)
			{
				TraceQueryableContractCreation(context);

				var result = new BadRequestContract();
				result.Errors.Add(new BadRequestErrorContract
				{
					Type = RequestValidationStates.ValidationFailure.ToString(),
					Message = notSupportedException.Message
				});

				context.Result = new ObjectResult(result)
				{
					StatusCode = (int) HttpStatusCode.BadRequest
				};

				return;
			}

			context.HttpContext.Items.Add(nameof(QueryableContract), queryContract);
			await next();
		}

		private static void TraceQueryableContractCreation(ActionContext actionContext)
		{
			actionContext.HttpContext.RequestServices.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace("Attempt to create Queryable Contract instance failed.", SeverityLevel.Warning, actionContext.HttpContext.GetTelemetryProperties());
		}
    }
}