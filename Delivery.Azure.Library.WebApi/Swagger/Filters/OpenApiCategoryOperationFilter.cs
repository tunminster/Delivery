using System.Reflection;
using Delivery.Azure.Library.WebApi.Swagger.Attributes;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Delivery.Azure.Library.WebApi.Swagger.Filters
{
    internal class OpenApiCategoryOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                var swaggerOperationCategoryAttribute = controllerActionDescriptor.MethodInfo.GetCustomAttribute<SwaggerCategoryAttribute>() ??
                                                        controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<SwaggerCategoryAttribute>();

                if (swaggerOperationCategoryAttribute != null)
                {
                    operation.Extensions[SwaggerCategoryAttribute.CategoryVendorExtensionName] = new OpenApiString(swaggerOperationCategoryAttribute.Category);
                }
            }
        }
    }
}