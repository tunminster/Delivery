using System;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;

namespace Delivery.Database.Entities
{
    public class NotificationDevice : Entity , IAuditableEntity, ISoftDeleteEntity
    {
        public string RegistrationId { get; set; }
        
        public string Platform { get; set; }
        
        public string Tag { get; set; }
        public string InsertedBy { get; set; }
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}