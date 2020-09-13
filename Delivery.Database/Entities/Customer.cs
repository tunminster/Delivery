using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Delivery.Database.Models;

namespace Delivery.Database.Entities
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string IdentityId { get; set; }

        [MaxLength(256)]
        public string Username { get; set; }
        public ApplicationUser Identity { get; set; }  // navigation property

        public virtual ICollection<Address> Addresses { get; set; }
    }
}