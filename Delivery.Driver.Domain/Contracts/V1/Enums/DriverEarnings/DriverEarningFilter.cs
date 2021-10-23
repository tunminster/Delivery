using System.Runtime.Serialization;

namespace Delivery.Driver.Domain.Contracts.V1.Enums.DriverEarnings
{
    public enum DriverEarningFilter
    {
        [EnumMember] None = 0,
        [EnumMember] Monthly = 1,
        [EnumMember] Yearly = 2
    }
}