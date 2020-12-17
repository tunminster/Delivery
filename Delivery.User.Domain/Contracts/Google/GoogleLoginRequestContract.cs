using System.ComponentModel.DataAnnotations;
using System.Resources;
using System.Runtime.Serialization;

namespace Delivery.User.Domain.Contracts.Google
{
    [DataContract]
    public class GoogleLoginRequestContract
    {
        [Required]
        [StringLength(255)]
        [DataMember]
        public string IdToken { get; set; }
        
        [Required]
        [DataMember]
        public string Provider { get; set; }
    }
}