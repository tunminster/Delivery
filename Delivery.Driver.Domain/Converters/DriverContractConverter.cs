using System.Linq;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile;
using Microsoft.Graph;
using Nest;

namespace Delivery.Driver.Domain.Converters
{
    public static class DriverContractConverter
    {
        public static Database.Entities.Driver ConvertToEntity(DriverCreationContract driverCreationContract, DriverCreationStatusContract driverCreationStatusContract)
        {
            var driver = new Database.Entities.Driver
            {
                ExternalId = driverCreationStatusContract.DriverId,
                FullName = driverCreationContract.FullName,
                EmailAddress = driverCreationContract.EmailAddress,
                VehicleType = driverCreationContract.VehicleType,
                DrivingLicenseNumber = driverCreationContract.DrivingLicenseNumber,
                SocialSecurityNumber = driverCreationContract.SocialSecurityNumber,
                DrivingLicenseExpiryDate = driverCreationContract.DrivingLicenseExpiryDate,
                BankName = driverCreationContract.BankName,
                BankAccountNumber = driverCreationContract.BankAccountNumber,
                RoutingNumber = driverCreationContract.RoutingNumber,
                ImageUri = driverCreationStatusContract.ImageUri,
                DrivingLicenseFrontUri = driverCreationStatusContract.DrivingLicenseFrontUri,
                DrivingLicenseBackUri = driverCreationStatusContract.DrivingLicenseBackUri,
                Radius = driverCreationContract.Radius,
                Latitude = driverCreationContract.Latitude,
                Longitude = driverCreationContract.Longitude,
                AddressLine1 = driverCreationContract.AddressLine1,
                AddressLine2 = driverCreationContract.AddressLine2,
                City = driverCreationContract.City,
                County = driverCreationContract.County,
                Country = driverCreationContract.Country
            };

            return driver;
        }

        public static DriverProfileContract ConvertToDriverProfile(this Database.Entities.Driver driver)
        {
            var addressProperties = new[]
            {
                driver.AddressLine1,
                driver.City,
                driver.County
            };
            
            var driverProfileContract = new DriverProfileContract
            {
                DriverId = driver.ExternalId,
                FullName = driver.FullName,
                EmailAddress = driver.EmailAddress,
                Address = string.Join(",", addressProperties.Where(x => !string.IsNullOrEmpty(x))),
                ImageUri = driver.ImageUri
            };

            return driverProfileContract;
        }

        public static DriverContract ConvertToDriverContract(this Database.Entities.Driver driver)
        {
            var driverContract = new DriverContract
            {
                DriverId = driver.ExternalId,
                EmailAddress = driver.EmailAddress,
                FullName = driver.FullName,
                ImageUri = driver.ImageUri,
                IsActive = driver.IsActive,
                Location = new GeoLocation(driver.Latitude, driver.Longitude),
                Radius = driver.Radius,
                VehicleType = driver.VehicleType,
                Approved = driver.Approved
            };

            return driverContract;
        }
    }
}