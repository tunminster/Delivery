using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Azure.Library.Database.Entities.V1;
using Delivery.Database.Models;

namespace Delivery.Database.Entities
{
    public class Customer : Entity
    {
        public string IdentityId { get; set; }

        [MaxLength(256)]
        public string Username { get; set; }

        [ForeignKey("IdentityId")]
        public ApplicationUser Identity { get; set; }  
        public virtual ICollection<Address> Addresses { get; set; }
    }
}