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
        public string StripePaymentMethodId { get; set; }
        
        [MaxLength(250)]
        public string PaymentStatus { get; set; }
        
        public bool Captured { get; set; }
        
        public long? AmountCaptured { get; set; }
        
        [MaxLength(250)]
        public string FailureCode { get; set; }
        
        [MaxLength(500)]
        public string FailureMessage { get; set; }
        
        public DateTimeOffset CapturedDateTime { get; set; }
        
        [MaxLength(500)]
        public string ReceiptUrl { get; set; }

        [MaxLength(250)]
        public string InsertedBy { get; set; }
        
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
        
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
    }
}