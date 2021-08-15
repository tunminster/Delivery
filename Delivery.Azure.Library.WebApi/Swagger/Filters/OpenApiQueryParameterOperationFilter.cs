using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Delivery.Azure.Library.WebApi.OData;
using Delivery.Azure.Library.WebApi.Swagger.Attributes;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Delivery.Azure.Library.WebApi.Swagger.Filters
{
    public class OpenApiQueryParameterOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (!(context.ApiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor))
            {
                return;
            }

            var swaggerParameterAttributes =
                controllerActionDescriptor.MethodInfo.GetCustomAttributes<SwaggerQueryParameterAttribute>() ??
                controllerActionDescriptor.ControllerTypeInfo.GetCustomAttributes<SwaggerQueryParameterAttribute>();

            var parameterAttributes = swaggerParameterAttributes.ToList();

            operation.Parameters ??= new List<OpenApiParameter>();

            parameterAttributes.ForEach(attribute =>
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = GetDisplayName(attribute.Name!).Keys.First(),
                    Description = GetDisplayName(attribute.Name!).Values.First(),
                    In = ParameterLocation.Query,
                    Required = attribute.Required,
                    Schema = new OpenApiSchema {Type = attribute.DataType},
                    AllowReserved = true
                });
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "culture",
                Description = "Allows to pass 2 digit culture to get localized result",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema { Type = "string", Default = new OpenApiString("en") }
            });
        }

        private static Dictionary<string, string> GetDisplayName(string propertyName)
        {
            MemberInfo? property = typeof(QueryableContract).GetProperty(propertyName);
            var displayName = property!.GetCustomAttribute<DisplayAttribute>()?.Name;
            var displayDescription = property!.GetCustomAttribute<DisplayAttribute>()?.Description;

            return new Dictionary<string, string>
            {
                {displayName!, displayDescription!}
            };
        }
    }
}