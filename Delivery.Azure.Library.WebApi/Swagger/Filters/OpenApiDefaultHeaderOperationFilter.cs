using System;
using System.Linq;
using System.Reflection;
using Delivery.Azure.Library.Configuration.Features.Interfaces;
using Delivery.Azure.Library.Core;
using Delivery.Azure.Library.WebApi.Swagger.Attributes;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.VisualStudio.Threading;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Delivery.Azure.Library.WebApi.Swagger.Filters
{
    internal class OpenApiDefaultHeaderOperationFilter : IOperationFilter
	{
		private readonly IServiceProvider serviceProvider;

		public OpenApiDefaultHeaderOperationFilter(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			if (!(context.ApiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor))
			{
				return;
			}

			var isAuthorizationRequired = true;
			var swaggerQueryTypeAttribute = controllerActionDescriptor.MethodInfo.GetCustomAttribute<ClientRestrictedAuthorizationAttribute>() ??
			                                controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<ClientRestrictedAuthorizationAttribute>();

			if (swaggerQueryTypeAttribute?.IsAuthorizationHeaderOptional == true)
			{
				// authorization security may or may not be there. there is no way to mark security as optional so remove it and allow CloudFlare/api gateway to manage restrictions
				isAuthorizationRequired = false;
			}

			AddHeader(operation, "Authorization", "Required to authenticate via OAuth to the api", isRequired: isAuthorizationRequired, "Bearer {{bearerToken}}");
			AddHeader(operation, HttpHeaders.CorrelationId, "Allows to pass a correlation id to the service", isRequired: false);
			AddHeader(operation, HttpHeaders.OcpSubscriptionKey, "Allows to pass a ocp subscription key to the service", isRequired: false);

			var featureProvider = serviceProvider.GetRequiredService<IFeatureProvider>();

			// https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1538
			// not possible to call Apply async
			var isEnabled = new JoinableTaskContext().Factory.Run(async () => await featureProvider.IsEnabledAsync("ApiRingOverride", isEnabledByDefault: false));
			if (isEnabled)
			{
				AddHeader(operation, HttpHeaders.Ring, "Allows to set a target ring if the environment allows it", isRequired: false);
			}
		}

		private static void AddHeader(OpenApiOperation operation, string key, string description, bool isRequired, string? valueOverride = null)
		{
			if (operation.Parameters.Any(p => p.Name == key))
			{
				return;
			}

			var openApiParameter = new OpenApiParameter
			{
				Name = key,
				In = ParameterLocation.Header,
				Description = description,
				Required = isRequired,
				Schema = new OpenApiSchema
				{
					Type = "string",
					Default = new OpenApiString(valueOverride ?? $"{{{{{key}}}}}")
				}
			};

			operation.Parameters.Add(openApiParameter);
		}
	}
}