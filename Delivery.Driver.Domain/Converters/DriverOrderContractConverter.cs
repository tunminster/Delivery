using Delivery.Database.Entities;
using Delivery.Domain.Helpers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrder;

namespace Delivery.Driver.Domain.Converters
{
    public static class DriverOrderContractConverter
    {
        public static DriverOrderDetailsContract ConvertToDriverOrderDetailsContract(this DriverOrder driverOrder)
        {
            var driverOrderDetailsContract = new DriverOrderDetailsContract
            {
                OrderId = driverOrder.Order.ExternalId,
                StoreId = driverOrder.Order.Store.ExternalId,
                StoreName = driverOrder.Order.Store.StoreName,
                StoreAddress = driverOrder.Order.Store.FormattedAddress ?? string.Empty,
                DeliveryAddress = FormatAddressLinesHelper.FormatAddress(driverOrder.Order.Address.AddressLine,
                    string.Empty, driverOrder.Order.Address.City, string.Empty,
                    driverOrder.Order.Address.Country, driverOrder.Order.Address.PostCode),
                DeliveryFee = driverOrder.Order.DeliveryFees,
                Tips = 0
            };

            return driverOrderDetailsContract;
        }
    }
}