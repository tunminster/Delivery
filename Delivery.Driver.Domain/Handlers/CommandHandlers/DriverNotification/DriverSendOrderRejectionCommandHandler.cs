using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Delivery.Azure.Library.NotificationHub.Clients;
using Delivery.Azure.Library.NotificationHub.Models;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverNotifications;
using Delivery.Notifications.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverNotification
{
    public record DriverSendOrderRejectionCommand(
        DriverOrderRejectedNotificationContract DriverOrderRejectedNotificationContract, string DriverId);
    
    public class DriverSendOrderRejectionCommandHandler : ICommandHandler<DriverSendOrderRejectionCommand, StatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public DriverSendOrderRejectionCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StatusContract> HandleAsync(DriverSendOrderRejectionCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var driver = await databaseContext.Drivers.SingleOrDefaultAsync(x => x.ExternalId == command.DriverId) 
                         ?? throw new InvalidOperationException($"Expected a driver by id: {command.DriverId} ");
            
            var driverDevice =
                databaseContext.NotificationDevices.FirstOrDefault(x => x.UserEmail == driver.EmailAddress) ??
                throw new InvalidOperationException($"Driver - {driver.EmailAddress} hasn't registered notification feature");
            
            var notificationSendModel = new NotificationSendModel<DriverOrderRejectedNotificationContract>
            {
                Pns = driverDevice.Platform,
                Message = "Notify food delivery rejection",
                Data = command.DriverOrderRejectedNotificationContract,
                ToTag = driverDevice.Tag,
                Username = driver.EmailAddress,
                CorrelationId = executingRequestContextAdapter.GetCorrelationId(),
                ShardKey = executingRequestContextAdapter.GetShard().Key,
                RingKey = executingRequestContextAdapter.GetRing().ToString()
            };
            
            var notificationClient = await NotificationClient.CreateAsync(serviceProvider, NotificationHubConstants.NotificationDriverHubName, NotificationHubConstants.NotificationDriverHubConnectionStringName);
            
            var statusCode = await notificationClient.SendNotificationToUser(notificationSendModel);
            if (statusCode == HttpStatusCode.Accepted || statusCode == HttpStatusCode.Created || statusCode == HttpStatusCode.OK)
            {
                return new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow };
            }
            
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackException(new InvalidOperationException($"Driver order rejection push notification failed - Driver name {driver.EmailAddress} and Order id {command.DriverOrderRejectedNotificationContract.OrderId}"),
                executingRequestContextAdapter.GetTelemetryProperties());
            
            return new StatusContract { Status = false, DateCreated = DateTimeOffset.UtcNow };
        }
    }
}