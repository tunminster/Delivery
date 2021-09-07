using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Database.Enums;
using Delivery.Domain.CommandHandlers;
using Delivery.Shop.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Converters;
using Delivery.Store.Domain.Factories.GeoLocationFactory;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopCreation
{
    public record ShopCreationCommand(ShopCreationContract ShopCreationContract, ShopCreationStatusContract ShopCreationStatusContract);
    
    public class ShopCreationCommandHandler : ICommandHandler<ShopCreationCommand, ShopCreationStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;

        public ShopCreationCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ShopCreationStatusContract> Handle(ShopCreationCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var store = ShopContractConverter.ConvertToEntity(command.ShopCreationContract, command.ShopCreationStatusContract);
            
            var storeType =
                await databaseContext.StoreTypes.FirstOrDefaultAsync(x =>
                    x.ExternalId == command.ShopCreationContract.StoreTypeId);
            if (storeType == null)
            {
                throw new InvalidOperationException(
                    $"Empty store type returns for store type {command.ShopCreationContract.StoreTypeId}");
            }
            
            store.StoreTypeId = storeType.Id;
            store.OpeningHours = new List<OpeningHour>();
            
            foreach (var storeOpeningHour in command.ShopCreationContract.StoreOpeningHours)
            {
                store.OpeningHours.Add(new OpeningHour
                {
                    DayOfWeek = storeOpeningHour.DayOfWeek,
                    Open = storeOpeningHour.Open,
                    Close = storeOpeningHour.Close,
                    IsDeleted = false,
                    InsertedBy = command.ShopCreationContract.EmailAddress,
                    InsertionDateTime = DateTimeOffset.UtcNow
                });
            }
            
            // add payment account number
            if (!string.IsNullOrEmpty(command.ShopCreationContract.PaymentAccountNumber))
            {
                store.StorePaymentAccount = new StorePaymentAccount
                {
                    AccountNumber = command.ShopCreationContract.PaymentAccountNumber,
                    IsDeleted = false
                };
            }
            
            store.StoreUsers.Add(new StoreUser
            {
                Username = command.ShopCreationContract.EmailAddress,
                UserStoreRole = UserStoreRole.Owner,
                IsDeleted = false,
                InsertedBy = command.ShopCreationContract.EmailAddress,
                InsertionDateTime = DateTimeOffset.UtcNow
            });
            
            await databaseContext.Stores.AddAsync(store);
            await databaseContext.SaveChangesAsync();
            
            await new StoreGeoLocationFactory(serviceProvider, executingRequestContextAdapter)
                .PublishStoreGeoUpdateMessageAsync(store.ExternalId);

            return command.ShopCreationStatusContract;
        }
    }
}