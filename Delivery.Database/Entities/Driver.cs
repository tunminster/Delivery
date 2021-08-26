using System;
using System.ComponentModel.DataAnnotations;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Entities.V1;
using Delivery.Database.Enums;

namespace Delivery.Database.Entities
{
    public class Driver : Entity , IAuditableEntity, ISoftDeleteEntity
    {
        [MaxLength(250)]
        public string FullName { get; set; }
        
        [MaxLength(250)]
        public string EmailAddress { get; set; }
        
        public VehicleType VehicleType { get; set; }
        
        [MaxLength(250)]
        public string DrivingLicenseNumber { get; set; }
        
        [MaxLength(50)]
        public string SocialSecurityNumber { get; set; }
        
        public DateTimeOffset DrivingLicenseExpiryDate { get; set; }
        
        [MaxLength(250)]
        public string BankName { get; set; }
        
        [MaxLength(50)]
        public string BankAccountNumber { get; set; }
        
        [MaxLength(50)]
        public string RoutingNumber { get; set; }
        
        [MaxLength(500)]
        public string ImageUri { get; set; }
        
        [MaxLength(500)]
        public string DrivingLicenseFrontUri { get; set; }
        
        [MaxLength(500)]
        public string DrivingLicenseBackUri { get; set; }
        
        [MaxLength(250)]
        public string ServiceArea { get; set; }

        public int Radius { get; set; }
        
        public double Latitude { get; set; }
        
        public double Longitude { get; set; }
        
        public string AddressLine1 { get; set; }
        
        public string AddressLine2 { get; set; }
        
        public string City { get; set; }
        
        public string County { get; set; }
        
        public string Country { get; set; }
        
        public bool Approved { get; set; }
        
        public bool IsActive { get; set; }
        
        [MaxLength(50)]
        public string InsertedBy { get; set; }
        public DateTimeOffset InsertionDateTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}