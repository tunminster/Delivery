using System;
using System.Runtime.Serialization;

namespace Delivery.Azure.Library.Microservices.Hosting.Enums
{
    [DataContract]
    [Flags]
    public enum HostTypes
    {
        [EnumMember] None = 0,

        /// <summary>
        ///     Represents a request-response host
        /// </summary>
        [EnumMember] WebHost = 1,

        /// <summary>
        ///     Represents a host that listens to a service bus
        /// </summary>
        [EnumMember] MessagingHost = 2
    }
}