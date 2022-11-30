using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;

namespace Delivery.Database.Entities
{
    public class OrderItemMeatOption : Entity, IAuditableEntity, ISoftDeleteEntity
    {
        public int OrderItemId { get; set; }
        
        public int MeatOptionId { get; set; }
        
        public string MeatOptionText { get; set; }
        
        [ForeignKey("OrderItemId")]
        public virtual OrderItem OrderItem { get; set; }
        public string InsertedBy { get; set; }
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
        
        public virtual ICollection<OrderItemMeatOptionValue> OrderItemMeatOptionValues { get; set; }
    }
}