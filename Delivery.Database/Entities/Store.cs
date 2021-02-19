using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;

namespace Delivery.Database.Entities
{
    public class Store : Entity , IAuditableEntity, ISoftDeleteEntity
    {
        [MaxLength(500)]
        public string StoreName { get; set; }
        
        [MaxLength(500)]
        public string ImageUri { get; set; }
        
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
        
        [MaxLength(50)]
        public string PostalCode { get; set; }
        
        public double? Latitude { get; set; }
        
        public double? Longitude { get; set; }
        
        [MaxLength(500)]
        public string? FormattedAddress { get; set; }
        
        public int StoreTypeId { get; set; }
        
        [ForeignKey("StoreTypeId")]
        public virtual StoreType StoreType { get; set; }
        
        [MaxLength(250)]
        public string InsertedBy { get; set; }
        
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
        
        public virtual ICollection<OpeningHour> OpeningHours { get; set; }
    }
}