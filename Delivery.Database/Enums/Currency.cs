using System;
using System.Runtime.Serialization;

namespace Delivery.Database.Enums
{
    [DataContract]
    public enum Currency
    {
        [EnumMember] None =0,
        [EnumMember] BritishPound=1,
        [EnumMember] US = 2

    }
}
