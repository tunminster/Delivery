using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;

namespace Delivery.Database.Entities
{
    public class StripePayment : Entity , IAuditableEntity, ISoftDeleteEntity
    {
        public int OrderId { get; set; }
        
        [MaxLength(250)]
        public string StripePaymentIntentId { get; set; }

        [MaxLength(250)]
        public string  StripeClientSecret { get; set; }

        [MaxLength(250)]
        public string InsertedBy { get; set; }
        
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
        
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
    }
}