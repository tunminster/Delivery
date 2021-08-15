using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.WebApi.Swagger.Attributes;

namespace Delivery.Api.OpenApi
{
    /// <summary>
    ///     Specifies the category of an operation or controller.
    ///     Each category will generate an own Swagger specification.
    /// </summary>
    public class PlatformSwaggerCategoryAttribute : SwaggerCategoryAttribute
    {
        public PlatformSwaggerCategoryAttribute(ApiCategory apiCategory) : base(apiCategory.ToString())
        {
        }
    }
}