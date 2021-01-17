using System;
using System.ComponentModel.DataAnnotations;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;

namespace Delivery.Database.Entities
{
    public class StoreType : Entity , IAuditableEntity, ISoftDeleteEntity
    {
        [MaxLength(255)]
        public string StoreTypeName { get; set; }
        
        [MaxLength(500)]
        public string ImageUri { get; set; }
        
        [MaxLength(255)]
        public string InsertedBy { get; set; }
        
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}