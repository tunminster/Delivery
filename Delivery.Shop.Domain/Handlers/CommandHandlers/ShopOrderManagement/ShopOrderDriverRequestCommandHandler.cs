using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.NotificationHub.Contracts.Enums;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Database.Enums;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Domain.Helpers;
using Delivery.Driver.Domain.Handlers.QueryHandlers.DriverAssignment;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopOrderManagement
{
    public record ShopOrderDriverRequestCommand(ShopOrderDriverRequestContract ShopOrderDriverRequestContract);
    
    public class ShopOrderDriverRequestCommandHandler : ICommandHandler<ShopOrderDriverRequestCommand, StatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public ShopOrderDriverRequestCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StatusContract> Handle(ShopOrderDriverRequestCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var order = await databaseContext.Orders
                            .Include(x => x.Store)
                            .Include(x => x.Address)
                            .SingleOrDefaultAsync(x => x.ExternalId == command.ShopOrderDriverRequestContract.OrderId) ??
                         throw new InvalidOperationException($"Expected order by {command.ShopOrderDriverRequestContract.OrderId}.");

            var latitude = order.Store.Latitude ??
                           throw new InvalidOperationException($"Expected latitude: instead {order.ConvertToJson()}");
            var longitude = order.Store.Longitude ??
                           throw new InvalidOperationException($"Expected longitude: instead {order.ConvertToJson()}");
            var driverByNearestLocationQuery = new DriverByNearestLocationQuery
            {
                Latitude = latitude,
                Longitude = longitude,
                Distance = order.Store.Radius != null ? $"{order.Store.Radius}km" : "20km",
                Page = 1,
                PageSize = 10
            };

            var driverList = await new DriverByNearestLocationQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(driverByNearestLocationQuery);

            var driverContract = driverList.FirstOrDefault();

            if (driverContract == null)
            {
                serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"Expected to find a driver: instead {driverContract.ConvertToJson()}");
                return new StatusContract { Status = false, DateCreated = DateTimeOffset.UtcNow };
            }

            var driver = databaseContext.Drivers.SingleOrDefault(x => x.ExternalId == driverContract.DriverId);

            if (driver == null) return new StatusContract { Status = false, DateCreated = DateTimeOffset.UtcNow };
            
            databaseContext.DriverOrders.Add(new DriverOrder
                { DriverId = driver.Id, OrderId = order.Id, Status = DriverOrderStatus.None });
            
            // push notification to driver
            var shopOrderDriverRequestPushNotificationContract = new ShopOrderDriverRequestPushNotificationContract
            {
                OrderId = order.ExternalId,
                StoreName = order.Store.StoreName,
                StoreImageUri = order.Store.ImageUri,
                StoreAddress = order.Store.FormattedAddress!,
                DeliveryAddress = FormatAddressLinesHelper.FormatAddress(order.Address.AddressLine,
                    string.Empty, order.Address.City, string.Empty,
                    order.Address.Country, order.Address.PostCode),
                DeliveryFee = order.DeliveryFees,
                PushNotificationType = PushNotificationType.DeliveryRequest,
                DeliveryTips = 0
            };

            var statusContract = await SendPushNotificationAsync(shopOrderDriverRequestPushNotificationContract, driver.Id);

            return statusContract;

        }

        private async Task<StatusContract> SendPushNotificationAsync(ShopOrderDriverRequestPushNotificationContract shopOrderDriverRequestPushNotificationContract, int driverId)
        {
            var shopOrderDriverRequestPushNotificationCommand =
                new ShopOrderDriverRequestPushNotificationCommand(shopOrderDriverRequestPushNotificationContract,
                    driverId);

            var statusContract =
                await new ShopOrderDriverRequestPushNotificationCommandHandler(serviceProvider,
                        executingRequestContextAdapter)
                    .Handle(shopOrderDriverRequestPushNotificationCommand);
            
            return statusContract;
        }
    }
}