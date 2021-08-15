using System;

namespace Delivery.Azure.Library.WebApi.Swagger.Attributes
{
    /// <summary>
    ///     Specifies the category of an operation or controller.
    ///     Each category will generate an own Swagger specification.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SwaggerCategoryAttribute : Attribute
    {
        public const string CategoryVendorExtensionName = "x-category";

        public string Category { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SwaggerCategoryAttribute" /> class.
        /// </summary>
        /// <param name="category">The category name.</param>
        public SwaggerCategoryAttribute(string category)
        {
            Category = category;
        }
    }
}