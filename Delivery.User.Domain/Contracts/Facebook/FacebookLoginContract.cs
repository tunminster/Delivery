using System.ComponentModel.DataAnnotations;

namespace Delivery.User.Domain.Contracts.Facebook
{
    public class FacebookLoginContract
    {
        [Required]
        [StringLength(255)]
        public string FacebookToken { get; set; }
        
        [Required]
        public string Provider { get; set; }
    }
}