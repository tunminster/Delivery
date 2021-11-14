using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile;

namespace Delivery.Driver.Domain.Converters
{
    public static class DriverProfileConverter
    {
        public static DriverServiceAreaContract ConvertToDriverServiceAreaContract(this Database.Entities.Driver driver)
        {
            var driverServiceAreaContract = new DriverServiceAreaContract
            {
                ServiceArea = driver.ServiceArea,
                Radius = driver.Radius,
                Latitude = driver.Latitude,
                Longitude = driver.Longitude
            };

            return driverServiceAreaContract;
        }
    }
}