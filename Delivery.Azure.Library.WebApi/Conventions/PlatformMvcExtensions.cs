using Delivery.Azure.Library.Configuration.Environments.Enums;
using Delivery.Azure.Library.Core.Extensions.Collections;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.WebApi.Filters;
using Delivery.Azure.Library.WebApi.Swagger.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.WebApi.Conventions
{
    public static class PlatformMvcExtensions
	{
		/// <summary>
		///     Adds the Mvc services and configures standard json setters
		///     Dependencies:
		///     [None]
		///     Settings:
		///     [None]
		/// </summary>
		public static IServiceCollection AddPlatformMvcServices(this IServiceCollection services, IConfiguration configuration)
		{
			services
				.AddAntiforgery(
					options =>
					{
						options.Cookie.Name = "_af";
						options.Cookie.HttpOnly = true;
						options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
						options.HeaderName = "X-XSRF-TOKEN";
					})
				.AddResponseCompression(options => options.EnableForHttps = true)
				.AddControllers(options =>
				{
					options.SslPort = 443;
					options.Filters.Add(new CorrelationIdFilterAttribute());
					options.Filters.Add(new UrlEncodingFilterAttribute());
					options.Filters.Add(new ModelValidationFilterAttribute());
					options.Filters.Add(new JsonPayloadSchemaAttribute());
					options.Filters.Add(new LocalDevelopmentFilterAttribute());
					options.Conventions.Add(new ControllerDocumentationConvention());
				})
				.AddJsonOptions(options =>
				{
					var defaultJsonSerializerOptions = JsonExtensions.GetDefaultJsonSerializerOptions();
					defaultJsonSerializerOptions.Converters.ForEach(p => options.JsonSerializerOptions.Converters.Add(p));

					options.JsonSerializerOptions.PropertyNamingPolicy = defaultJsonSerializerOptions.PropertyNamingPolicy;

					var environment = configuration.GetValue<RuntimeEnvironment>("ASPNETCORE_ENVIRONMENT");
					if (environment == RuntimeEnvironment.Dev || environment == RuntimeEnvironment.Sbx)
					{
						options.JsonSerializerOptions.WriteIndented = true;
					}
				})
				.AddXmlDataContractSerializerFormatters();

			return services;
		}

		public static IApplicationBuilder UsePlatformMvcServices(this IApplicationBuilder applicationBuilder, bool useAuthorization = true)
		{
			applicationBuilder.UseRouting();

			if (useAuthorization)
			{
				applicationBuilder.UseAuthorization().UseAuthentication();
			}

			applicationBuilder.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });

			return applicationBuilder;
		}
	}
}