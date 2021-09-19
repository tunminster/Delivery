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
using Delivery.Driver.Domain.Contracts.V1.MessageContracts;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverActive;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverAssignment;
using Delivery.Driver.Domain.Handlers.MessageHandlers;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverActive;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverAssignment;
using Delivery.Order.Domain.Contracts.V1.MessageContracts;
using Delivery.Order.Domain.Handlers.MessageHandlers;
using Delivery.Order.Domain.Handlers.MessageHandlers.OrderUpdates;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopCreation;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopOrderManagement;
using Delivery.Shop.Domain.Handlers.MessageHandlers.ShopCreation;
using Delivery.Shop.Domain.Handlers.MessageHandlers.ShopOrderManagement;
using Delivery.Store.Domain.Contracts.V1.MessageContracts.StoreCreations;
using Delivery.Store.Domain.Contracts.V1.MessageContracts.StoreGeoUpdates;
using Delivery.Store.Domain.Contracts.V1.MessageContracts.StoreTypeCreations;
using Delivery.Store.Domain.Contracts.V1.MessageContracts.StoreUpdate;
using Delivery.Store.Domain.ElasticSearch.Contracts.V1.MessageContracts.StoreIndexing;
using Delivery.Store.Domain.ElasticSearch.Handlers.MessageHandlers.StoreIndexing;
using Delivery.Store.Domain.Handlers.MessageHandlers.StoreCreation;
using Delivery.Store.Domain.Handlers.MessageHandlers.StoreGeoUpdates;
using Delivery.Store.Domain.Handlers.MessageHandlers.StoreTypeCreations;
using Delivery.Store.Domain.Handlers.MessageHandlers.StoreUpdate;
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
                case nameof(StoreUpdateMessage):
                    var storeUpdateMessage = message.Deserialize<StoreUpdateMessage>();
                    var storeUpdateMessageHandler = new StoreUpdateMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(storeUpdateMessage.RequestContext));
                    await storeUpdateMessageHandler.HandleMessageAsync(storeUpdateMessage, processingState);
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
                case nameof(StoreIndexingMessageContract):
                    var storeIndexingMessageContract = message.Deserialize<StoreIndexingMessageContract>();
                    var storeIndexMessageHandler = new StoreIndexingMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(storeIndexingMessageContract.RequestContext));
                    await storeIndexMessageHandler.HandleMessageAsync(storeIndexingMessageContract, processingState);
                    break;
                
                case nameof(DriverCreationMessageContract):
                    var driverCreationMessageContract = message.Deserialize<DriverCreationMessageContract>();
                    var driverCreationMessageHandler = new DriverCreationMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(driverCreationMessageContract.RequestContext));
                    await driverCreationMessageHandler.HandleMessageAsync(driverCreationMessageContract,
                        processingState);
                    break;
                case nameof(DriverAssignmentMessageContract):
                    var driverAssignmentMessageContract = message.Deserialize<DriverAssignmentMessageContract>();
                    var driverAssignmentMessageHandler = new DriverAssignmentMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(driverAssignmentMessageContract.RequestContext));
                    await driverAssignmentMessageHandler.HandleMessageAsync(driverAssignmentMessageContract,
                        processingState);
                    break;
                case nameof(DriverActiveMessageContract):
                    var driverActiveMessageContract = message.Deserialize<DriverActiveMessageContract>();
                    var driverActiveMessageHandler = new DriverActiveMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(driverActiveMessageContract.RequestContext));
                    await driverActiveMessageHandler.HandleMessageAsync(driverActiveMessageContract, processingState);
                    break;
                case nameof(ShopCreationMessageContract):
                    var shopCreationMessageContract = message.Deserialize<ShopCreationMessageContract>();
                    var shopCreationMessageHandler = new ShopCreationMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(shopCreationMessageContract.RequestContext));
                    await shopCreationMessageHandler.HandleMessageAsync(shopCreationMessageContract, processingState);
                    break;
                case nameof(ShopOrderStatusMessageContract):
                    var shopOrderStatusMessageContract = message.Deserialize<ShopOrderStatusMessageContract>();
                    var shopOrderStatusMessageHandler = new ShopOrderStatusMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(shopOrderStatusMessageContract.RequestContext));
                    await shopOrderStatusMessageHandler.HandleMessageAsync(shopOrderStatusMessageContract, processingState);
                    break;
                case nameof(ShopOrderDriverRequestMessageContract):
                    var shopOrderDriverRequestContract = message.Deserialize<ShopOrderDriverRequestMessageContract>();
                    var shopOrderDriverRequestMessageHandler = new ShopOrderDriverRequestMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(shopOrderDriverRequestContract.RequestContext));
                    await shopOrderDriverRequestMessageHandler.HandleMessageAsync(shopOrderDriverRequestContract,
                        processingState);
                    break;
                default:
                    throw new NotImplementedException($"Message type {messageType} is not implemented.");
            }
        }
    }
}