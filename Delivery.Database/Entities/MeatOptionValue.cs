using System;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;

namespace Delivery.Database.Entities
{
    public class MeatOptionValue : Entity, IAuditableEntity, ISoftDeleteEntity
    {
        public string OptionValueText { get; set; }
        
        public int AdditionalPrice { get; set; }
        
        public int MeatOptionId { get; set; }
        
        [ForeignKey("MeatOptionId")]
        public virtual MeatOption MeatOption { get; set; }
        public string InsertedBy { get; set; }
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}