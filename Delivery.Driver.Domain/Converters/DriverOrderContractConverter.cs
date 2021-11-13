using Delivery.Database.Entities;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverHistory;

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

        public static DriverOrderHistoryContract ConvertToDriverHistoryContract(this DriverOrder driverOrder)
        {
            var driverHistoryContract = new DriverOrderHistoryContract
            {
                StoreName = driverOrder.Order.Store.StoreName,
                OrderId = driverOrder.Order.ExternalId,
                OrderDate = driverOrder.InsertionDateTime,
                DeliveryFee = driverOrder.Order.DeliveryFees
            };

            return driverHistoryContract;
        }
    }
}