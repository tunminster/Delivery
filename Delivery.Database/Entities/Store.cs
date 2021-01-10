using System;
using System.ComponentModel.DataAnnotations;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;

namespace Delivery.Database.Entities
{
    public class Store : Entity , IAuditableEntity, ISoftDeleteEntity
    {
        [MaxLength(500)]
        public string StoreName { get; set; }
        
        [MaxLength(500)]
        public string AddressLine1 { get; set; }
        
        [MaxLength(500)]
        public string AddressLine2 { get; set; }
        
        [MaxLength(250)]
        public string City { get; set; }
        
        [MaxLength(250)]
        public string County { get; set; }
        
        [MaxLength(250)]
        public string Country { get; set; }
        
        [MaxLength(250)]
        public string Latitude { get; set; }
        
        [MaxLength(250)]
        public string Longitude { get; set; }
        
        [MaxLength(250)]
        public string InsertedBy { get; set; }
        
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}