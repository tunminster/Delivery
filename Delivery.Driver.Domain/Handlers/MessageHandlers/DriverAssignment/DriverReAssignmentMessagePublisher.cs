using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.ConnectionManagement.HostedServices;
using Delivery.Azure.Library.Messaging.HostedServices.Interfaces;
using Delivery.Azure.Library.Messaging.ServiceBus.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.FrameWork.Messages;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverAssignment;

namespace Delivery.Driver.Domain.Handlers.MessageHandlers.DriverAssignment;

public class DriverReAssignmentMessagePublisher : IMessagePublisherAsync<DriverReAssignmentMessage>
{
    private readonly IServiceProvider serviceProvider;

    public DriverReAssignmentMessagePublisher(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }
    
    public async Task PublishAsync(DriverReAssignmentMessage message)
    {
        var executingContextAdapter = new ExecutingRequestContextAdapter(message.RequestContext);
        var cloudEventMessage = message
            .CreateCloudEventMessage(serviceProvider, executingContextAdapter)
            .WithScheduledEnqueueTimeUtc(DateTimeOffset.UtcNow.AddMinutes(3))  // schedule to 3 minutes later
            .WithExecutingContext(executingContextAdapter);
            
        await serviceProvider.GetRequiredHostedService<IQueueServiceBusWorkBackgroundService>()
            .EnqueueBackgroundWorkAsync(Delivery.Domain.Constants.Constants.DriverServiceBusEntityName,
                Delivery.Domain.Constants.Constants.DriverServiceBusConnectionStringName, cloudEventMessage);
    }
}