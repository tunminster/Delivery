using System;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;
using Delivery.Database.Enums;

namespace Delivery.Database.Entities
{
    public class DriverOrder : Entity, IAuditableEntity, ISoftDeleteEntity
    {
        public int OrderId { get; set; }
        
        public int DriverId { get; set; }
        
        public DriverOrderStatus Status { get; set; }
        public string InsertedBy { get; set; }
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
        
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
        
        [ForeignKey("DriverId")]
        public virtual Driver Driver { get; set; }
    }
}