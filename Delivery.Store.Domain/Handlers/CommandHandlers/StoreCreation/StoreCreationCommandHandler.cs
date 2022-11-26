using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Domain.CommandHandlers;
using Delivery.Store.Domain.Contracts.V1.MessageContracts.StoreGeoUpdates;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreGeoUpdate;
using Delivery.Store.Domain.Converters.StoreConverters;
using Delivery.Store.Domain.ElasticSearch.Contracts.V1.MessageContracts.StoreIndexing;
using Delivery.Store.Domain.ElasticSearch.Contracts.V1.RestContracts.StoreIndexing;
using Delivery.Store.Domain.ElasticSearch.Handlers.MessageHandlers.StoreIndexing;
using Delivery.Store.Domain.Factories.GeoLocationFactory;
using Delivery.Store.Domain.Handlers.MessageHandlers.StoreGeoUpdates;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

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
        
        public async Task<StoreCreationStatusContract> HandleAsync(StoreCreationCommand command)
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
            store.OpeningHours = new List<OpeningHour>();
            
            foreach (var storeOpeningHour in command.StoreCreationContract.StoreOpeningHours)
            {
                store.OpeningHours.Add(new OpeningHour
                {
                    DayOfWeek = storeOpeningHour.DayOfWeek,
                    Open = storeOpeningHour.Open,
                    Close = storeOpeningHour.Close,
                    IsDeleted = false,
                    InsertedBy = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail,
                    InsertionDateTime = DateTimeOffset.UtcNow
                });
            }

            // add payment account number
            store.StorePaymentAccount = new StorePaymentAccount
            {
                AccountNumber = command.StoreCreationContract.PaymentAccountNumber,
                IsDeleted = false
            };
            
            // store user
            var storeUser = new StoreUser
            {
                Username = command.StoreCreationContract.StoreUser.EmailAddress
            };
            
            store.StoreUsers = new List<StoreUser>();
            
            store.StoreUsers.Add(storeUser);
            
            await databaseContext.Stores.AddAsync(store);
            await databaseContext.SaveChangesAsync();

            await new StoreGeoLocationFactory(serviceProvider, executingRequestContextAdapter)
                .PublishStoreGeoUpdateMessageAsync(store.ExternalId);
            
            command.StoreCreationStatusContract.InsertionDateTime = store.InsertionDateTime;
            return command.StoreCreationStatusContract;
        }

        
        
    }
}