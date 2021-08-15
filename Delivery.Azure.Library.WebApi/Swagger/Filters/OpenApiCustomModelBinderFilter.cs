using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Delivery.Azure.Library.WebApi.Swagger.Filters
{
    public class OpenApiCustomModelBinderFilter : IOperationFilter
    {
        /// <summary>
        ///     Filter to handle multipart/form posts that take a contract object and collection of documents
        ///     Stops swagger defaulting the contract to the query string, allowing for accurate documentation output and "try it"
        ///     functionality in UI
        /// </summary>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var parameterDescriptions = context.ApiDescription.ParameterDescriptions;
            for (var i = 0; i < parameterDescriptions.Count; i++)
            {
                var parameterDescription = parameterDescriptions[i];
                var fullParamName = parameterDescription.Type.FullName;
                if (operation.RequestBody == null || !operation.RequestBody.Content.ContainsKey("multipart/form-data") || !fullParamName!.Contains("Contract"))
                {
                    continue;
                }

                var contractParameter = operation.Parameters[i];
                operation.RequestBody.Content["multipart/form-data"].Schema.Properties.Add(contractParameter.Name, contractParameter.Schema);
                operation.Parameters.RemoveAt(i);
            }
        }
    }
}