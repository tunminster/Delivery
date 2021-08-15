using System;

namespace Delivery.Azure.Library.WebApi.Swagger.Attributes
{
    /// <summary>
    ///     Indicates that an endpoint accepts multipart form uploads
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OpenApiMultipartUploadFilterAttribute : Attribute
    {
        public string DocumentName { get; }

        public OpenApiMultipartUploadFilterAttribute(string documentName = "documents")
        {
            DocumentName = documentName;
        }
    }
}