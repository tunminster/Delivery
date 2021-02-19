using System;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;

namespace Delivery.Database.Entities
{
    public class OpeningHour : Entity, IAuditableEntity, ISoftDeleteEntity
    {
        public string DayOfWeek { get; set; }
        
        public string Open { get; set; }
        
        public string Close { get; set; }
        
        public int StoreId { get; set; }
        
        public string InsertedBy { get; set; }
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
        
        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }
    }
}