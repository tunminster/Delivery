using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Store.Domain.Contracts.V1.MessageContracts.StoreGeoUpdates;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreGeoUpdate;
using Delivery.Store.Domain.Converters.StoreConverters;
using Delivery.Store.Domain.ElasticSearch.Contracts.V1.MessageContracts.StoreIndexing;
using Delivery.Store.Domain.ElasticSearch.Contracts.V1.RestContracts.StoreIndexing;
using Delivery.Store.Domain.ElasticSearch.Handlers.MessageHandlers.StoreIndexing;
using Delivery.Store.Domain.Handlers.MessageHandlers.StoreGeoUpdates;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Store.Domain.Handlers.CommandHandlers.StoreCreation
{
    public class StoreCreationCommandHandler : ICommandHandler<StoreCreationCommand, StoreCreationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public StoreCreationCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StoreCreationStatusContract> Handle(StoreCreationCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var store = StoreConverter.Convert(command.StoreCreationContract);

            var storeType =
                await databaseContext.StoreTypes.FirstOrDefaultAsync(x =>
                    x.ExternalId == command.StoreCreationContract.StoreTypeId);

            store.ExternalId = command.StoreCreationStatusContract.StoreId;
            store.InsertionDateTime = DateTimeOffset.UtcNow;
            store.InsertedBy = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail;
            store.IsDeleted = false;
            store.StoreTypeId = storeType.Id;

            await databaseContext.Stores.AddAsync(store);
            await databaseContext.SaveChangesAsync();

            await PublishStoreGeoUpdateMessageAsync(store.ExternalId);

            command.StoreCreationStatusContract.InsertionDateTime = store.InsertionDateTime;
            return command.StoreCreationStatusContract;
        }

        private async Task PublishStoreGeoUpdateMessageAsync(string storeId)
        {
            var storeGeoUpdateContract = new StoreGeoUpdateContract
            {
                StoreId = storeId
            };

            var storeGeoUpdateStatusContract = new StoreGeoUpdateStatusContract();

            var storeGeoUpdateMessageContract = new StoreGeoUpdateMessageContract
            {
                PayloadIn = storeGeoUpdateContract,
                PayloadOut = storeGeoUpdateStatusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };
            
            // Publish store geo location update
            await new StoreGeoUpdateMessagePublisher(serviceProvider).PublishAsync(storeGeoUpdateMessageContract);
            
            // Publish store indexing
            await PublishStoreIndexingAsync(storeGeoUpdateContract.StoreId);
            
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                .TrackTrace($"{nameof(PublishStoreGeoUpdateMessageAsync)} published store update geo message", SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());

        }

        private async Task PublishStoreIndexingAsync(string storeId)
        {
            var storeIndexCreationContract = new StoreIndexCreationContract
            {
                StoreId = storeId
            };

            var storeIndexStatusContract = new StoreIndexStatusContract
            {
                Status = false,
                InsertionDateTime = DateTimeOffset.UtcNow
            };

            var storeIndexingMessageContract = new StoreIndexingMessageContract
            {
                PayloadIn = storeIndexCreationContract,
                PayloadOut = storeIndexStatusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };
            
            await new StoreIndexingMessagePublisher(serviceProvider).PublishAsync(storeIndexingMessageContract);
        }
    }
}