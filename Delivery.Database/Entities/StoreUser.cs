using System;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;
using Delivery.Database.Enums;

namespace Delivery.Database.Entities
{
    public class StoreUser : Entity , IAuditableEntity, ISoftDeleteEntity
    {
        public int StoreId { get; set; }
        public string Username { get; set; }
        
        public bool Approved { get; set; }
        public UserStoreRole UserStoreRole { get; set; }
        public string InsertedBy { get; set; }
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
        
        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }
    }
}