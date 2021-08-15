using System;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Azure.Library.WebApi.Swagger.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class SwaggerQueryParameterAttribute : Attribute
    {
        public SwaggerQueryParameterAttribute(string name, string description, string dataType, bool required = false)
        {
            Name = name;
            Description = description;
            DataType = dataType;
            Required = required;
        }

        public string? Name { get; }
        public string? DataType { get; }
        public string? Description { get; }
        public bool Required { get; }

        public override string ToString()
        {
            return $"{GetType().Name}: " +
                   $"{nameof(Name)}: {Name?.Format()}, " +
                   $"{nameof(DataType)}: {DataType?.Format()}, " +
                   $"{nameof(Description)}: {Description?.Format()}, " +
                   $"{nameof(Required)} : {Required.Format()}, ";
        }
    }
}