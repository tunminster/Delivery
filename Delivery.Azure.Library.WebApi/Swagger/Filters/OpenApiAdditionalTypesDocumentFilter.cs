using System;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Delivery.Azure.Library.WebApi.Swagger.Filters
{
    internal class OpenApiAdditionalTypesDocumentFilter : IDocumentFilter
    {
        private readonly Type[] types;

        public OpenApiAdditionalTypesDocumentFilter(params Type[] types)
        {
            this.types = types;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var type in types)
            {
                if (!swaggerDoc.Components.Schemas.Keys.Contains(type.Name))
                {
                    context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository);
                }
            }
        }
    }
}