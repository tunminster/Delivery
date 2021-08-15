using System;
using System.Collections.Generic;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Delivery.Azure.Library.WebApi.Swagger.Filters
{
    internal class OpenApiAuthorizationHeaderOperationFilter : IOperationFilter
    {
        private readonly IServiceProvider serviceProvider;

        public OpenApiAuthorizationHeaderOperationFilter(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Responses.Add("401", new OpenApiResponse {Description = "Unauthorized"});
            operation.Responses.Add("403", new OpenApiResponse {Description = "Forbidden"});

            var audience = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSettingOrDefault("Okta-AuthorizationServer-Audience", "https://localhost");
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new()
                {
                    [
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            }
                        }
                    ] = new[] {audience}
                }
            };
        }
    }
}