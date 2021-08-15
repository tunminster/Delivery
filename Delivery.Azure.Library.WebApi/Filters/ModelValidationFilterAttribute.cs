using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Delivery.Azure.Library.WebApi.Filters
{
    public class ModelValidationFilterAttribute : ActionFilterAttribute
	{
		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			if (context.ModelState.IsValid)
			{
				await next();
				return;
			}

			var result = CreateResult(context);

			if (!result.Errors.Any())
			{
				await next();
				return;
			}

			context.Result = new ObjectResult(result)
			{
				StatusCode = (int) HttpStatusCode.BadRequest
			};
		}

		public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
		{
			if (context.Result is BadRequestObjectResult || context.Result is BadRequestResult)
			{
				var result = CreateResult(context);

				if (!result.Errors.Any())
				{
					await next();
					return;
				}

				context.Result = new ObjectResult(result)
				{
					StatusCode = (int) HttpStatusCode.BadRequest
				};
			}

			await next();
		}

		private static BadRequestContract CreateResult(ActionContext context)
		{
			// serialization failures contain the line number and position
			var isSerializationFailure = context.ModelState.Any(error => error.Value.Errors.Any(modelError => !string.IsNullOrEmpty(modelError.ErrorMessage) && modelError.ErrorMessage.Contains("The JSON value could not be converted to")));
			var errorName = isSerializationFailure ? RequestValidationStates.SerializationFailure.ToString() : RequestValidationStates.ValidationFailure.ToString();

			var result = new BadRequestContract();
			result.Errors.Add(new BadRequestErrorContract
			{
				Type = errorName,
				Message = CreateErrorMessage(context)
			});

			return result;
		}

		private static string CreateErrorMessage(ActionContext context)
		{
			return string.Join("\n", context.ModelState.Where(p => p.Value.Errors.Any()).Select(error =>
			{
				var childErrors = error.Value.Errors
					.Select(propertyError => !string.IsNullOrWhiteSpace(propertyError.ErrorMessage) ? propertyError.ErrorMessage : propertyError.Exception?.Message ?? "No error")
					.Select(e => e.Replace("\n", "\n  "));

				return error.Key + ": \n  " + string.Join("\n  ", childErrors);
			}));
		}
	}
}