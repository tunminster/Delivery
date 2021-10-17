using System.Runtime.Serialization;

namespace Delivery.Database.Enums
{
    public enum IdentityDocumentType
    {
        [EnumMember] None = 0,
        [EnumMember] Passport= 1,
        [EnumMember] DriverLicense = 2,
        [EnumMember] ResidentPermit = 3,
        [EnumMember] CitizenCard = 4,
        [EnumMember] ElectoralId = 5,
        [EnumMember] Other = 6
    }
}