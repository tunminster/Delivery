using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;
using Delivery.Database.Enums;

namespace Delivery.Database.Entities
{
    public class MeatOption : Entity, IAuditableEntity, ISoftDeleteEntity
    {
        public string OptionText { get; set; }
        
        public OptionControlType OptionControl { get; set; }
        
        public int ProductId { get; set; }
        
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
        public string InsertedBy { get; set; }
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
        
        public virtual ICollection<MeatOptionValue> MeatOptionValues { get; set; }
    }
}