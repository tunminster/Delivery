using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.ConnectionManagement.HostedServices;
using Delivery.Azure.Library.Messaging.HostedServices.Interfaces;
using Delivery.Azure.Library.Messaging.ServiceBus.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.Constants;
using Delivery.Domain.FrameWork.Messages;
using Delivery.Managements.Domain.Contracts.V1.MessageContracts.MeatOptionValues;

namespace Delivery.Managements.Domain.Handlers.MessageHandler;

public class MeatOptionValueCreationMessagePublisher : IMessagePublisherAsync<MeatOptionValueCreationMessage>
{
    private readonly IServiceProvider serviceProvider;

    public MeatOptionValueCreationMessagePublisher(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }
    
    public async Task PublishAsync(MeatOptionValueCreationMessage message)
    {
        var executingContextAdapter = new ExecutingRequestContextAdapter(message.RequestContext);
        
        var cloudEventMessage = message
            .CreateCloudEventMessage(serviceProvider, executingContextAdapter)
            .WithExecutingContext(executingContextAdapter);
            
        await serviceProvider.GetRequiredHostedService<IQueueServiceBusWorkBackgroundService>()
            .EnqueueBackgroundWorkAsync(OrderConstants.ServiceBusEntityName,
                OrderConstants.ServiceBusConnectionStringName, cloudEventMessage);
    }
}