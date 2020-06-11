using Delivery.Api.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Delivery.Api.Entities
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string IdentityId { get; set; }

        [MaxLength(256)]
        public string Username { get; set; }
        public ApplicationUser Identity { get; set; }  // navigation property

        public virtual IList<Address> Addresses { get; set; }
    }
}
