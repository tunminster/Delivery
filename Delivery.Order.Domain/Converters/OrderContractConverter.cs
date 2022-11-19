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
                CustomerName = order.Customer.Username,
                TotalAmount = order.TotalAmount -
                              (order.BusinessServiceFees + order.PlatformServiceFees + order.DeliveryFees),
                BusinessApplicationFees = order.BusinessServiceFees,
                OrderType = order.OrderType,
                StoreName = order.Store?.StoreName ?? string.Empty,
                Status = order.Status,
                DeliveryAddress = order.Address?.AddressLine ?? string.Empty,
                OrderItems = order.OrderItems?.Select(x => x.ConvertToOrderItemContract()).ToList() ?? new(),
                DateCreated = order.InsertionDateTime
            };

            return orderManagementContract;
        }
        
        public static OrderAdminManagementContract ConvertToOrderAdminManagementContract(this Database.Entities.Order order)
        {
            var orderAdminManagementContract = new OrderAdminManagementContract
            {
                Id = order.ExternalId,
                CustomerId = order.Customer.ExternalId,
                CustomerName = order.Customer.Username,
                TotalAmount = order.TotalAmount -
                              (order.BusinessServiceFees + order.PlatformServiceFees + order.DeliveryFees),
                BusinessApplicationFees = order.BusinessServiceFees,
                OrderType = order.OrderType,
                StoreName = order.Store?.StoreName ?? string.Empty,
                Status = order.Status,
                DeliveryAddress = order.Address?.AddressLine ?? string.Empty,
                OrderItems = order.OrderItems?.Select(x => x.ConvertToOrderItemContract()).ToList() ?? new(),
                DateCreated = order.InsertionDateTime,
                DeliveryFees = order.DeliveryFees,
                UpdateDate = order.DateUpdated,
                DeliveryRequested = order.DeliveryRequested,
                TaxFees = order.TaxFees,
                BusinessServiceFees = order.BusinessServiceFees,
                PlatformServiceFees = order.PlatformServiceFees
                
            };

            return orderAdminManagementContract;
        }

        public static OrderItemContract ConvertToOrderItemContract(this Database.Entities.OrderItem orderItem)
        {
            var orderItemContract = new OrderItemContract
            {
                ProductId = orderItem.ProductId,
                ProductName = orderItem.Product?.ProductName ?? string.Empty,
                ProductPrice = orderItem.Product?.UnitPrice ?? 100,
                Count = orderItem.Count
            };

            return orderItemContract;
        }
    }
}