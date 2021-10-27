using System.Runtime.Serialization;

namespace Delivery.Driver.Domain.Contracts.V1.Enums.DriverEarnings
{
    public enum DriverEarningFilter
    {
        [EnumMember] None = 0,
        [EnumMember] Weekly = 1,
        [EnumMember] Monthly = 2
    }
}