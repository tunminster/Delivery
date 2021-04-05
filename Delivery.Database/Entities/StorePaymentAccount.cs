using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;

namespace Delivery.Database.Entities
{
    public class StorePaymentAccount : Entity , IAuditableEntity, ISoftDeleteEntity
    {
        public int StoreId { get; set; }
        
        [MaxLength(500)]
        public string AccountNumber { get; set; }
        public string InsertedBy { get; set; }
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
        
        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }
    }
}