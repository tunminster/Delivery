using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;
using Delivery.Database.Models;

namespace Delivery.Database.Entities
{
    public class Customer : Entity, IAuditableEntity, ISoftDeleteEntity
    {
        public string IdentityId { get; set; }

        [MaxLength(256)]
        public string Username { get; set; }
        
        [MaxLength(256)]
        public string FirstName { get; set; }
        
        [MaxLength(256)]
        public string LastName { get; set; }
        
        [MaxLength(50)]
        public string ContactNumber { get; set; }

        [ForeignKey("IdentityId")]
        public ApplicationUser Identity { get; set; }  
        public virtual ICollection<Address> Addresses { get; set; }
        public string InsertedBy { get; set; }
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}