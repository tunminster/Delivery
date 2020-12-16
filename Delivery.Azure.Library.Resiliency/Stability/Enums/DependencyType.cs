using System.Runtime.Serialization;

namespace Delivery.Azure.Library.Resiliency.Stability.Enums
{
    [DataContract]
    public enum DependencyType
    {
        [EnumMember] None,
        [EnumMember] Api,
        [EnumMember] Storage,
        [EnumMember] Messaging,
        [EnumMember] Cosmos,
        [EnumMember] Email
    }
}