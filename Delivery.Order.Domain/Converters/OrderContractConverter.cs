using System.Linq;
using Delivery.Order.Domain.Contracts;
using Delivery.Order.Domain.Contracts.V1.RestContracts;

namespace Delivery.Order.Domain.Converters
{
    public static class OrderContractConverter
    {
        public static OrderManagementContract ConvertToOrderManagementContract(this Database.Entities.Order order)
        {
            var orderManagementContract = new OrderManagementContract
            {
                Id = order.ExternalId,
                CustomerId = order.Customer.ExternalId,
                TotalAmount = order.TotalAmount -
                              (order.BusinessServiceFees + order.PlatformServiceFees + order.DeliveryFees),
                BusinessApplicationFees = order.BusinessServiceFees,
                OrderType = order.OrderType,
                StoreName = order.Store.StoreName,
                DeliveryAddress = order.Address.AddressLine,
                OrderItems = order.OrderItems.Select(x => x.ConvertToOrderItemContract()).ToList(),
                DateCreated = order.InsertionDateTime
            };

            return orderManagementContract;
        }

        public static OrderItemContract ConvertToOrderItemContract(this Database.Entities.OrderItem orderItem)
        {
            var orderItemContract = new OrderItemContract
            {
                ProductId = orderItem.ProductId,
                ProductName = orderItem.Product.ProductName,
                ProductPrice = orderItem.Product.UnitPrice,
                Count = orderItem.Count
            };

            return orderItemContract;
        }
    }
}