using System;
using System.ComponentModel.DataAnnotations;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;
using Delivery.Database.Enums;

namespace Delivery.Database.Entities
{
    public class CouponCode : Entity, IAuditableEntity, ISoftDeleteEntity
    {
        [MaxLength(256)]
        public string Name { get; set; }
        
        [MaxLength(256)]
        public string CouponId { get; set; }
        
        [MaxLength(256)]
        public CouponCodeType CouponCodeType { get; set; }
        
        public DateTimeOffset RedeemBy { get; set; }
        
        public int NumberOfTimes { get; set; }
        
        public int MinimumOrderValue { get; set; }
        
        [MaxLength(256)]
        public string PromotionCode { get; set; }
        
        public int DiscountAmount { get; set; }
        public string InsertedBy { get; set; }
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}