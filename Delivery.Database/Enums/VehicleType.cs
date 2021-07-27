using System.Runtime.Serialization;

namespace Delivery.Database.Enums
{
    [DataContract]
    public enum VehicleType
    {
        [EnumMember] None = 0,
        [EnumMember] Bike = 1,
        [EnumMember] MotorBike = 2,
        [EnumMember] Car = 3
        
    }
}