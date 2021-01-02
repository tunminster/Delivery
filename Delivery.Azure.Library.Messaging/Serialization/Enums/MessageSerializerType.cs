using System.Runtime.Serialization;

namespace Delivery.Azure.Library.Messaging.Serialization.Enums
{
    [DataContract]
    public enum MessageSerializerType
    {
        [EnumMember] None,
        [EnumMember] DataContractSerializer,
        [EnumMember] Json
    }
}