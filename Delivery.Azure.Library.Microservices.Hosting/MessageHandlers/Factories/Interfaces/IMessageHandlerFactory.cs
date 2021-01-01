using System;
using System.Collections.Generic;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers.Interfaces;
using Microsoft.Azure.ServiceBus;

namespace Delivery.Azure.Library.Microservices.Hosting.MessageHandlers.Factories.Interfaces
{
    public interface IMessageHandlerFactory<in TMessageMetadata, in TState>
        where TState : Enum
    {
        IMessageHandler CreateMessageHandler(Message enqueueMessage, TState processingState, TMessageMetadata messageMetadata, string correlationId, IDictionary<string, string> messageProperties);
    }
}