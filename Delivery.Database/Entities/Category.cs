using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;

namespace Delivery.Database.Entities
{
    public class Category : Entity, IAuditableEntity, ISoftDeleteEntity
    {

        [MaxLength(300)]
        public string CategoryName { get; set; }

        [MaxLength(4000)]
        public string Description { get; set; }
        public int ParentCategoryId { get; set; }
        public int Order { get; set;  }
        
        public int? StoreId { get; set; }
        
        [MaxLength(250)]
        public string InsertedBy { get; set; }
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
        
        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }
    }
}