using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Delivery.Azure.Library.WebApi.Swagger.Attributes;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Delivery.Azure.Library.WebApi.Swagger.Filters
{
    public class OpenApiMultipartUploadFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (!(context.ApiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor))
            {
                return;
            }

            var multipartUploadFilterAttribute = controllerActionDescriptor.MethodInfo.GetCustomAttribute<OpenApiMultipartUploadFilterAttribute>() ??
                                                 controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<OpenApiMultipartUploadFilterAttribute>();

            if (multipartUploadFilterAttribute == null)
            {
                return;
            }

            var uploadFileMediaType = new OpenApiMediaType
            {
                Schema = new OpenApiSchema
                {
                    Type = "object",
                    Properties =
                    {
                        [multipartUploadFilterAttribute.DocumentName] = new OpenApiSchema
                        {
                            Type = "array",
                            Nullable = true,
                            Items = new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary"
                            }
                        }
                    }
                },
                Encoding = new Dictionary<string, OpenApiEncoding>
                {
                    {multipartUploadFilterAttribute.DocumentName, new OpenApiEncoding {Style = ParameterStyle.Form}}
                }
            };

            foreach (var openApiParameter in operation.Parameters.Where(p => p.In == ParameterLocation.Query).ToList())
            {
                uploadFileMediaType.Schema.Properties[openApiParameter.Name] = openApiParameter.Schema;
                operation.Parameters.Remove(openApiParameter);
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Content =
                {
                    ["multipart/form-data"] = uploadFileMediaType
                },
            };
        }
    }
}