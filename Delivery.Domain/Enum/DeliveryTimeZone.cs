using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Delivery.Domain.Enum
{
    [DataContract]
    public enum DeliveryTimeZone
    {
        
        [Display(Name="")][EnumMember] None = 0,
        [Display(Name="Eastern Standard Time")][EnumMember] Est = 1,
        [Display(Name="Central Standard Time")][EnumMember] Cst = 2,
        [Display(Name="Mountain Standard Time")][EnumMember] Mst = 3,
        [Display(Name="Pacific Standard Time")][EnumMember] Pst = 4,
        [Display(Name="Alaska Standard Time")][EnumMember] Ast = 5,
        [Display(Name="Hawaii-Aleutian Standard Time")][EnumMember] Hst = 6,
        [Display(Name="Greenwich Mean Time")][EnumMember] Gmt = 7
    }
}