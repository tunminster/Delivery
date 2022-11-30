using System;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;

namespace Delivery.Database.Entities
{
    public class OrderItemMeatOptionValue : Entity, IAuditableEntity, ISoftDeleteEntity
    {
        public int OrderItemMeatOptionId { get; set; }
        
        public int MeatOptionValueId { get; set; }
        
        public string MeatOptionValueText { get; set; }
        
        [ForeignKey("OrderItemMeatOptionId")]
        public virtual OrderItemMeatOption OrderItemMeatOption { get; set; }
        
        public string InsertedBy { get; set; }
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}