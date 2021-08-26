using System.Linq;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile;
using Microsoft.Graph;

namespace Delivery.Driver.Domain.Converters
{
    public static class DriverContractConverter
    {
        public static Database.Entities.Driver ConvertToEntity(DriverCreationContract driverCreationContract, DriverCreationStatusContract driverCreationStatusContract)
        {
            var driver = new Database.Entities.Driver
            {
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
                Longitude = driverCreationContract.Longitude
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
    }
}