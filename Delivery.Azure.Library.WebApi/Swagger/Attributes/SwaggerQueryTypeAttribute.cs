using System;

namespace Delivery.Azure.Library.WebApi.Swagger.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class SwaggerQueryTypeAttribute : Attribute
    {
        public SwaggerQueryTypeAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }
}