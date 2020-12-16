using System.Runtime.Serialization;

namespace Delivery.Azure.Library.Resiliency.Stability.Enums
{
    [DataContract]
    public enum ExternalDependency
    {
        [EnumMember] ServiceBus,
        [EnumMember] TableStorage,
        [EnumMember] BlobStorage,
        [EnumMember] PlatformDatabase,
        [EnumMember] KeyVault,
        [EnumMember] Cosmos,
        [EnumMember] SendGrid
    }
}