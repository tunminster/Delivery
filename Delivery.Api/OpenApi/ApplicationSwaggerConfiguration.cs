using System;
using System.Linq;
using System.Reflection;
using Delivery.Address.Domain.Documentation;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Configuration.Environments.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Conventions.Configurations;
using Delivery.Customer.Domain.Documentation;
using Delivery.Driver.Domain.Documentation;
using Delivery.StoreOwner.Domain.Documentation;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Api.OpenApi
{
    public class ApplicationSwaggerConfiguration : PlatformSwaggerConfiguration
	{
		public ApplicationSwaggerConfiguration(IServiceProvider serviceProvider) : base(serviceProvider)
		{
			AddXmlDocumentationPaths();
			var environment = serviceProvider.GetRequiredService<IEnvironmentProvider>().GetCurrentEnvironment().ToString();
			AddRouteWithCategoryFilter(ApiCategory.Management, ManagementApiDocumentation.GetApiGeneralDocumentationMarkdown(environment, ApiCategory.Management.ToString()));
			AddRouteWithCategoryFilter(ApiCategory.Driver, DriverApiDocumentation.GetApiGeneralDocumentationMarkdown(environment, ApiCategory.Driver.ToString()));
			AddRouteWithCategoryFilter(ApiCategory.ShopOwner, StoreOwnerApiDocumentation.GetApiGeneralDocumentationMarkdown(environment, ApiCategory.ShopOwner.ToString()));
			AddRouteWithCategoryFilter(ApiCategory.Customer, CustomerApiDocumentation.GetApiGeneralDocumentationMarkdown(environment, ApiCategory.Customer.ToString()));
			AddRouteWithCategoryFilter(ApiCategory.WebApp, WebAppApiDocumentation.GetApiGeneralDocumentationMarkdown(environment, ApiCategory.WebApp.ToString()));
		}

		private void AddRouteWithCategoryFilter(ApiCategory apiCategory, string description, bool sortBySummary = false)
		{
			AddRouteWithCategoryFilterAndPathTransformation($"{apiCategory.ToString()} Apis", apiCategory.ToString(), $"api/{apiCategory.ToString().ToLowerInvariant()}", $"api/v1/{apiCategory.ToString().ToLowerInvariant()}", sortBySummary, description: description);
		}

		private void AddXmlDocumentationPaths()
		{
			var entryAssembly = typeof(Program).Assembly;
			var telemetryAssembly = typeof(BadRequestContract).Assembly;

			AddXmlDocumentationPath(entryAssembly);
			AddXmlDocumentationPath(telemetryAssembly);

			var domainAssemblies = entryAssembly.GetReferencedAssemblies().Where(assemblyName => !string.IsNullOrEmpty(assemblyName.Name) && assemblyName.Name.Contains("Delivery.") && assemblyName.Name.Contains(".Domain"));
			if (!domainAssemblies.Any())
			{
				throw new InvalidOperationException($"Could not find any assemblies matching the pattern for Domain assemblies from entry assembly {entryAssembly.FullName}");
			}

			foreach (var domainAssembly in domainAssemblies)
			{
				var assembly = Assembly.Load(domainAssembly);
				AddXmlDocumentationPath(assembly);
			}
		}
	}
}