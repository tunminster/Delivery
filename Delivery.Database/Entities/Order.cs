using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;
using Delivery.Database.Enums;

namespace Delivery.Database.Entities
{
    public class Order : Entity, IAuditableEntity, ISoftDeleteEntity
    {

        [MaxLength(300)]
        public string Description { get; set; }

        public int TotalAmount { get; set; }

        [MaxLength(15)]
        public string CurrencyCode { get; set; }

        [MaxLength(15)]
        public string PaymentType { get; set; }
        
        [MaxLength(15)]
        public string PaymentStatus { get; set; }
        
        [MaxLength(50)]
        public string PaymentIntentId { get; set; }
        
        [MaxLength(500)]
        public string PaymentAccountNumber { get; set; }

        public int CustomerId { get; set; }
        
        public int? PreparationTime { get; set; }
        
        public OrderPaymentStatus PaymentStatusCode { get; set; }
        

        public int? AddressId { get; set; }
        
        public int? StoreId { get; set; }
        
        public int SubTotal { get; init; }
        
        public int BusinessServiceFees { get; init; }
        
        public int PlatformServiceFees { get; init; }
        
        public int DeliveryFees { get; init; }
        
        public int TaxFees { get; init; }
        
        public OrderStatus Status { get; set; }
        
        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }
        
        [ForeignKey("AddressId")]
        public virtual Address Address { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
        
        public string InsertedBy { get; set; }
        
        public DateTimeOffset InsertionDateTime { get; set; }
        
        public DateTimeOffset DateUpdated { get; set; }
        
        public DateTimeOffset? OrderReadyDateTime { get; set; }
        
        public DateTimeOffset? PickupTime { get; set; }
        
        public DateTimeOffset? OrderAcceptedDateTime { get; set; }
        
        public DateTimeOffset? DeliveryEstimatedDateTime { get; set; }
        
        public DateTimeOffset? DeliveredDateTime { get; set; }

        public int DeliveryRequested { get; set; }
        
        public OrderType OrderType { get; set; }
        public bool IsDeleted { get; set; }
    }
}