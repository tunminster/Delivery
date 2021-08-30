using System.Runtime.Serialization;

namespace Delivery.Database.Enums
{
    [DataContract]
    public enum UserStoreRole
    {
        [EnumMember] None = 0,
        [EnumMember] Owner = 1,
        [EnumMember] User = 2
    }
}