using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;

namespace Delivery.Database.Entities
{
    public class Product : Entity, IAuditableEntity, ISoftDeleteEntity
    {

        [MaxLength(300)]
        public string ProductName { get; set; }

        [MaxLength(4000)]
        public string Description { get; set; }
        public string ProductImage { get; set; }
        public string ProductImageUrl { get; set; }
        
        public int UnitPrice { get; set; }
        
        [MaxLength(50)]
        public string Currency { get; set; }
        [MaxLength(20)]
        public string CurrencySign { get; set; }
        public int CategoryId { get; set; }
        
        public int StoreId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }
        
        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        [MaxLength(255)]
        public string InsertedBy { get; set; }
        
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}