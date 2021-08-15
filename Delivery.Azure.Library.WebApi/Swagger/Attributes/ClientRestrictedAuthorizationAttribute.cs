using System;

namespace Delivery.Azure.Library.WebApi.Swagger.Attributes
{
    /// <summary>
    ///     Indicates that the endpoint may be secured by additional means other than just with authorization
    /// </summary>
    /// <example>When the client UI is public and IP address restrictions are in place</example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class ClientRestrictedAuthorizationAttribute : Attribute
    {
        public ClientRestrictedAuthorizationAttribute(bool isAuthorizationHeaderOptional = false)
        {
            IsAuthorizationHeaderOptional = isAuthorizationHeaderOptional;
        }

        public bool IsAuthorizationHeaderOptional { get; }
    }
}