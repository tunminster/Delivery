using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;

namespace Delivery.Database.Entities
{
    public class CouponCodeCustomer : Entity , IAuditableEntity, ISoftDeleteEntity
    {
        [MaxLength(256)]
        public string PromotionCode { get; set; }
        
        [MaxLength(256)]
        public string Username { get; set; }
        
        public int CouponCodeId { get; set; }

        public string InsertedBy { get; set; }
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
        
        [ForeignKey("CouponCodeId")]
        public CouponCode CouponCode { get; set; }  
    }
}