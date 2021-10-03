using System;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;

namespace Delivery.Database.Entities
{
    public class DriverPayment : Entity, IAuditableEntity, ISoftDeleteEntity
    {
        public int TotalPaymentAmount { get; set; }
        
        public int DriverId { get; set; }
        public string InsertedBy { get; set; }
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}