using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Json;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Delivery.Azure.Library.WebApi.Filters
{
    public class JsonPayloadSchemaAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var isJsonRequest = context.HttpContext.Request.ContentType?.Length > 0 && (context.HttpContext.Request.ContentType?.ToLowerInvariant().Contains("application/json") ?? false);

            if (isJsonRequest)
            {
                var bodyParameter = context.ActionDescriptor?.Parameters?.SingleOrDefault(parameterDescriptor => parameterDescriptor.BindingInfo?.BindingSource == BindingSource.Body);
                if (bodyParameter == null)
                {
                    await next();
                    return;
                }

                var targetObject = context.ActionArguments.Single(keyValuePair => keyValuePair.Key == bodyParameter.Name).Value;

                context.HttpContext.Request.Body.Position = 0;
                using var jsonDocument = await JsonDocument.ParseAsync(context.HttpContext.Request.Body);
                jsonDocument.RootElement.ValidateSchema(targetObject);
            }

            await next();
        }
    }
}