using System;
using System.Collections.Generic;
using System.Linq;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Configuration.Environments.Enums;
using Delivery.Azure.Library.Configuration.Environments.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.WebApi.Conventions.Configurations;
using Delivery.Azure.Library.WebApi.Conventions.Configurations.Interfaces;
using Delivery.Azure.Library.WebApi.Swagger;
using Delivery.Azure.Library.WebApi.Swagger.Attributes;
using Delivery.Azure.Library.WebApi.Swagger.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using ServiceStack;

namespace Delivery.Azure.Library.WebApi.Conventions
{
    public static class PlatformSwaggerExtensions
	{
		/// <summary>
		///     Configures swagger documentation and rewrites the root URL to the Swagger UI.
		///     Dependencies:
		///     <see cref="Configuration.Configurations.Interfaces.IConfigurationProvider" />
		///     Settings:
		///     <see cref="PlatformSwaggerConfiguration" />
		/// </summary>
		/// <exception cref="ArgumentException">Specified API documentation was not found.</exception>
		// ReSharper disable once UnusedMethodReturnValue.Global
		public static IServiceCollection AddPlatformSwaggerServices<TSwaggerConfiguration>(this IServiceCollection services, Dictionary<string, string> scopes)
			where TSwaggerConfiguration : class, IPlatformSwaggerConfiguration
		{
			services.AddSingleton<IPlatformSwaggerConfiguration, TSwaggerConfiguration>();
			services.AddSwaggerGen(options =>
			{
				var serviceProvider = services.BuildServiceProvider();
				var swaggerConfiguration = serviceProvider.GetRequiredService<IPlatformSwaggerConfiguration>();

				if (!swaggerConfiguration.Routes.Any())
				{
					swaggerConfiguration.AddRouteWithAllOperations();
				}

				options.SwaggerDoc("v1",
					new OpenApiInfo
					{
						Title = swaggerConfiguration.Title,
						Version = "v1",
						Description = swaggerConfiguration.Description
					}
				);

				foreach (var documentationPath in swaggerConfiguration.DocumentationPaths)
				{
					options.IncludeXmlComments(documentationPath);
				}

				var authorizationUrl = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSettingOrDefault("Okta-AuthorizationServer-AuthorizeEndpoint", "https://localhost:5001");
				var tokenUrl = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSettingOrDefault("Okta-AuthorizationServer-TokenEndpoint", "https://localhost:5001");

				options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
				{
					Type = SecuritySchemeType.OAuth2,
					Flows = new OpenApiOAuthFlows
					{
						AuthorizationCode = new OpenApiOAuthFlow
						{
							AuthorizationUrl = new Uri(authorizationUrl),
							TokenUrl = new Uri(tokenUrl),
							Scopes = scopes
						}
					}
				});

				options.OperationFilter<OpenApiCategoryOperationFilter>();
				options.OperationFilter<OpenApiQueryParameterOperationFilter>();
				options.OperationFilter<OpenApiQueryTypeOperationFilter>();
				options.OperationFilter<OpenApiDefaultHeaderOperationFilter>();
				options.OperationFilter<OpenApiAuthorizationHeaderOperationFilter>();
				options.OperationFilter<OpenApiCustomModelBinderOperationFilter>();
				options.OperationFilter<OpenApiMultipartUploadFilter>();

				var additionalTypes = (object) swaggerConfiguration.Routes
					.SelectMany(route => route.AdditionalTypes)
					.Distinct()
					.ToArray();

				options.DocumentFilter<OpenApiAdditionalTypesDocumentFilter>(additionalTypes);
				options.DocumentFilter<OpenApiExampleDocumentFilter>();

				options.UseInlineDefinitionsForEnums();
				options.TagActionsBy(description =>
				{
					// allow action name to override controller
					var controllerActionDescriptor = description.ActionDescriptor as ControllerActionDescriptor;
					return new List<string>
					{
						controllerActionDescriptor?.AttributeRouteInfo?.Name ?? controllerActionDescriptor?.ControllerName ?? string.Empty
					};
				});

			});

			return services;
		}

		/// <summary>
		///     Configures swagger documentation and rewrites the root url to the swagger ui
		///     Dependencies:
		///     [None]
		///     Settings:
		///     [None]
		/// </summary>
		// ReSharper disable once UnusedMethodReturnValue.Global
		public static IApplicationBuilder UsePlatformSwaggerServices(this IApplicationBuilder applicationBuilder)
		{
			var swaggerConfiguration = applicationBuilder.ApplicationServices.GetRequiredService<IPlatformSwaggerConfiguration>();
			foreach (var route in swaggerConfiguration.Routes)
			{
				applicationBuilder.UseSwagger(options =>
				{
					options.PreSerializeFilters.Add((document, _) => UpdateDocumentMetadata(document, route, swaggerConfiguration));
					options.PreSerializeFilters.Add((document, _) => ApplyCategoryFilter(document, route));
					options.PreSerializeFilters.Add((document, _) => RemoveUnusedDefinitionSchemas(document, route));

					if (!string.IsNullOrWhiteSpace(route.RouteSegment))
					{
						options.RouteTemplate = $"swagger/{route.RouteSegment}/{{documentName}}/swagger.json";
					}

					if (!string.IsNullOrWhiteSpace(route.PathPrefix))
					{
						options.PreSerializeFilters.Add((document, _) => TransformOperationPaths(applicationBuilder, document, route));
					}
				});
			}

			applicationBuilder.UseSwaggerUI(options =>
			{
				foreach (var route in swaggerConfiguration.Routes)
				{
					var url = !string.IsNullOrWhiteSpace(route.RouteSegment) ? $"{route.RouteSegment}/v1/swagger.json" : "v1/swagger.json";
					var title = GetTitle(route, swaggerConfiguration);

					options.SwaggerEndpoint(url, title);
				}
			});

			var rewriteOptions = new RewriteOptions().AddRedirect("^$", "swagger");
			applicationBuilder.UseRewriter(rewriteOptions);

			return applicationBuilder;
		}

		private static string GetTitle(SwaggerRoute route, IPlatformSwaggerConfiguration swaggerConfiguration)
		{
			return !string.IsNullOrWhiteSpace(route.TitleSuffix) ? $"{swaggerConfiguration.Title} - {route.TitleSuffix}" : swaggerConfiguration.Title;
		}

		private static string GetDescription(SwaggerRoute route, IPlatformSwaggerConfiguration swaggerConfiguration)
		{
			return !string.IsNullOrWhiteSpace(route.Description) ? route.Description : swaggerConfiguration.Description;
		}

		private static void UpdateDocumentMetadata(OpenApiDocument document, SwaggerRoute route, IPlatformSwaggerConfiguration swaggerConfiguration)
		{
			document.Info.Title = GetTitle(route, swaggerConfiguration);
			document.Info.Description = GetDescription(route, swaggerConfiguration);
		}

		private static void TransformOperationPaths(IApplicationBuilder applicationBuilder, OpenApiDocument document, SwaggerRoute route)
		{
			var serviceProvider = applicationBuilder.ApplicationServices;
			var apiBaseUrl = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSettingOrDefault("Api-Base-Url", string.Empty);
			if (!string.IsNullOrEmpty(apiBaseUrl))
			{
				document.Servers = new List<OpenApiServer>
				{
					new()
					{
						Url = $"{apiBaseUrl}",
						Description = "The base url to construct the full request url from"
					}
				};
			}

			foreach (var path in document.Paths.ToList())
			{
				var originalPath = path.Key;
				var value = path.Value;

				var isApiGatewayRequest = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext?.Request.Query.ContainsKey("isApiGatewayDefinition") ?? false;
				var isLocalDevelopment = serviceProvider.GetRequiredService<IEnvironmentProvider>().IsEnvironment(RuntimeEnvironment.Dev);

				var usedPath = originalPath;
				if (isApiGatewayRequest)
				{
					// the api gateway overrides the base url as apis cannot share the same base url
					// however this is a special case as normal open api docs expect the base url and generated endpoints to be correct
					usedPath = originalPath.StartsWith($"/{route.PathPrefix}/") ? originalPath.Substring((route.PathPrefix?.Length ?? 0) + 1) : originalPath;
				}

				if (isLocalDevelopment)
				{
					// when debugging locally there is no ng-inx ingress to route to the rings, so use the redirected root url
					var isRunningInContainerOrchestrator = !string.IsNullOrEmpty(serviceProvider.GetRequiredService<IConfigurationProvider>().GetSettingOrDefault("Node_Name", string.Empty));
					if (isRunningInContainerOrchestrator)
					{
						// if running locally (with ingress, without api gateway, in kubernetes) then direct calls to the first ring for testing
						usedPath = $"/ring0{originalPath}";
					}
				}

				document.Paths.Remove(originalPath);
				var openApiPathItem = new OpenApiPathItem
				{
					Summary = value.Summary,
					Extensions = value.Extensions,
					Servers = value.Servers,
					Operations = value.Operations,
					Parameters = value.Parameters,
					Description = value.Description
				};

				document.Paths.Add(usedPath, openApiPathItem);
			}
		}

		private static void ApplyCategoryFilter(OpenApiDocument document, SwaggerRoute route)
		{
			foreach (var path in Enumerable.ToArray(document.Paths))
			{
				var pathItem = path.Value;

				foreach (var pathItemOperation in pathItem.Operations.ToList())
				{
					ModifyOrRemoveOperation(route.Category, pathItemOperation.Value, () => pathItem.Operations.Remove(pathItemOperation));
				}

				if (!pathItem.Operations.Any())
				{
					document.Paths.Remove(path.Key);
				}
			}
		}

		private static void RemoveUnusedDefinitionSchemas(OpenApiDocument document, SwaggerRoute route)
		{
			bool hasDocumentChanged;
			do
			{
				hasDocumentChanged = false;

				var additionalTypeKeys = route.AdditionalTypes.Select(p => p.Name).ToList();

				var documentJson = document.ToJson();
				foreach (var schema in Enumerable.ToArray(document.Components.Schemas))
				{
					if (!additionalTypeKeys.Contains(schema.Key))
					{
						var referencePath = $"\"#/components/schemas/{schema.Key}\"";
						if (!documentJson.Contains(referencePath))
						{
							document.Components.Schemas.Remove(schema);
							hasDocumentChanged = true;
						}
					}
				}
			} while (hasDocumentChanged); // repeat looking for unused schemas until no more changes are detected (used to remove transitive refs)
		}

		private static void ModifyOrRemoveOperation(string? category, IOpenApiExtensible operation, Action removeOperation)
		{
			var filterByCategory = !string.IsNullOrWhiteSpace(category);
			var operationHasCategory = operation.Extensions.ContainsKey(SwaggerCategoryAttribute.CategoryVendorExtensionName);
			if (operationHasCategory)
			{
				var categoryValue = operation.Extensions[SwaggerCategoryAttribute.CategoryVendorExtensionName] as OpenApiString;
				if (filterByCategory && (categoryValue == null || !string.Equals(categoryValue.Value, category, StringComparison.InvariantCultureIgnoreCase)))
				{
					removeOperation();
				}

				operation.Extensions.Remove(SwaggerCategoryAttribute.CategoryVendorExtensionName);
			}
			else if (filterByCategory)
			{
				removeOperation();
			}
		}
	}
}