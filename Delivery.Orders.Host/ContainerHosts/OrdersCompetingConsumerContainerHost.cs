using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Azure.Library.Messaging.Extensions;
using Delivery.Azure.Library.Messaging.Serialization;
using Delivery.Azure.Library.Messaging.ServiceBus;
using Delivery.Azure.Library.Microservices.Hosting.Hosts;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Metrics;
using Delivery.Domain.Contracts.Enums;
using Delivery.Order.Domain.Contracts.V1.MessageContracts;
using Delivery.Order.Domain.Handlers.MessageHandlers;
using Delivery.Order.Domain.Handlers.MessageHandlers.OrderUpdates;
using Delivery.Store.Domain.Contracts.V1.MessageContracts.StoreCreations;
using Delivery.Store.Domain.Contracts.V1.MessageContracts.StoreGeoUpdates;
using Delivery.Store.Domain.Contracts.V1.MessageContracts.StoreTypeCreations;
using Delivery.Store.Domain.Handlers.MessageHandlers.StoreCreation;
using Delivery.Store.Domain.Handlers.MessageHandlers.StoreGeoUpdates;
using Delivery.Store.Domain.Handlers.MessageHandlers.StoreTypeCreations;
using Delivery.StripePayment.Domain.Contracts.V1.MessageContracts;
using Delivery.StripePayment.Domain.Handlers.MessageHandlers;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Delivery.Orders.Host.ContainerHosts
{
    public class OrdersCompetingConsumerContainerHost : CompetingConsumerContainerHost<OrderMessageProcessingStates>
    {
        public OrdersCompetingConsumerContainerHost(IHostBuilder hostBuilder) : base(hostBuilder)
        {
        }

        public override string QueueOrTopicName => ServiceProvider.GetRequiredService<IConfigurationProvider>()
            .GetSetting("Topic_Name").ToLowerInvariant();

        protected override async Task ProcessMessageAsync(Message message, string correlationId, ApplicationInsightsStopwatch stopwatch,
            Dictionary<string, string> telemetryContextProperties)
        {
            var processingState = message.GetMessageProcessingState<OrderMessageProcessingStates>();
            var ring = message.GetRing();

            if (Ring != ring)
            {
                throw new InvalidOperationException($"The ring specified in the message {ring} does not match the ring which this {GetType().Name} can handle (ring: {Ring}). Properties: {telemetryContextProperties.Format()}");
            }

            if (!message.UserProperties.TryGetValue(UserProperties.MessageType, out var messageType))
            {
                throw new InvalidOperationException($"Message needs to have a {UserProperties.MessageType} header");
            }

            switch (messageType)
            {
                case nameof(OrderCreationMessage):
                    var orderCreationMessage = message.Deserialize<OrderCreationMessage>();
                    var orderCreationMessageHandler = new OrderCreationMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(orderCreationMessage.RequestContext));
                    await orderCreationMessageHandler.HandleMessageAsync(orderCreationMessage, processingState);
                    break;
                case nameof(PaymentCreationMessageContract):
                    var paymentCreationMessage = message.Deserialize<PaymentCreationMessageContract>();
                    var paymentCreationMessageHandler = new PaymentCreationMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(paymentCreationMessage.RequestContext));
                    await paymentCreationMessageHandler.HandleMessageAsync(paymentCreationMessage, processingState);
                    break;
                case nameof(OrderUpdateMessage):
                    var orderUpdateMessage = message.Deserialize<OrderUpdateMessage>();
                    var orderUpdateMessageHandler = new OrderUpdateMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(orderUpdateMessage.RequestContext));
                    await orderUpdateMessageHandler.HandleMessageAsync(orderUpdateMessage, processingState);
                    break;
                case nameof(StoreCreationMessageContract):
                    var storeCreationMessage = message.Deserialize<StoreCreationMessageContract>();
                    var storeCreationMessageHandler = new StoreCreationMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(storeCreationMessage.RequestContext));
                    await storeCreationMessageHandler.HandleMessageAsync(storeCreationMessage, processingState);
                    break;
                case nameof(StoreTypeCreationMessageContract):
                    var storeTypeCreationMessage = message.Deserialize<StoreTypeCreationMessageContract>();
                    var storeTypeCreationMessageHandler = new StoreTypeCreationMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(storeTypeCreationMessage.RequestContext));
                    await storeTypeCreationMessageHandler.HandleMessageAsync(storeTypeCreationMessage, processingState);
                    break;
                case nameof(StoreGeoUpdateMessageContract):
                    var storeGeoUpdateMessage = message.Deserialize<StoreGeoUpdateMessageContract>();
                    var storeGeoUpdateMessageHandler = new StoreGeoUpdateMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(storeGeoUpdateMessage.RequestContext));
                    await storeGeoUpdateMessageHandler.HandleMessageAsync(storeGeoUpdateMessage, processingState);
                    break;
                default:
                    throw new NotImplementedException($"Message type {messageType} is not implemented.");
            }
        }
    }
}