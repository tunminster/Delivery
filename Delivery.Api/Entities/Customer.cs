using Delivery.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Delivery.Api.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public string IdentityId { get; set; }
        public string Username { get; set; }
        public ApplicationUser Identity { get; set; }  // navigation property

        public virtual IList<Address> Addresses { get; set; }
    }
}
