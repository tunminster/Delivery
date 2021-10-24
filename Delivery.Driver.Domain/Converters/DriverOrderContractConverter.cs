using Delivery.Database.Entities;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;

namespace Delivery.Driver.Domain.Converters
{
    public static class DriverOrderContractConverter
    {
        public static DriverOrderContract ConvertToDriverOrderContract(this DriverOrder driverOrder,
            Database.Entities.Order order, Database.Entities.Driver driver)
        {
            var driverOrderContract = new DriverOrderContract
            {
                Id = driverOrder.ExternalId,
                DriverId = driver.ExternalId,
                OrderId = order.ExternalId,
                DeliveryFee = order.DeliveryFees,
                DateCreated = driverOrder.InsertionDateTime,
                Status = driverOrder.Status
            };

            return driverOrderContract;
        }
    }
}