using Delivery.Driver.Domain.Contracts.V1.RestContracts;

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
                Latitude = driverCreationContract.Latitude,
                Longitude = driverCreationContract.Longitude
            };

            return driver;
        }
    }
}