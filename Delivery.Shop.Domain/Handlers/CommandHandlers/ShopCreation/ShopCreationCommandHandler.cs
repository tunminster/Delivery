using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Database.Enums;
using Delivery.Domain.CommandHandlers;
using Delivery.Shop.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Converters;
using Delivery.Store.Domain.Factories.GeoLocationFactory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

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
        
        public async Task<ShopCreationStatusContract> HandleAsync(ShopCreationCommand command)
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
            store.IsActive = true;
            store.ContactNumber = command.ShopCreationContract.PhoneNumber;
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
            
            store.StoreUsers = new List<StoreUser>();
            
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

            await CreateStripeConnectAccountAsync(store.ExternalId, command.ShopCreationContract.EmailAddress,
                databaseContext);

            return command.ShopCreationStatusContract;
        }
        
        private async Task CreateStripeConnectAccountAsync(string storeExternalId, string shopOwnerEmailAddress, PlatformDbContext platformDbContext)
        {
            var stripeApiKey = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Stripe-{executingRequestContextAdapter.GetShard().Key}-Api-Key");
            StripeConfiguration.ApiKey = stripeApiKey;

            var options = new AccountCreateOptions
            {
                Country = "US",
                //Type = "express",
                Type = "custom",
                Capabilities = new AccountCapabilitiesOptions
                {
                    Transfers = new AccountCapabilitiesTransfersOptions
                    {
                        Requested = true
                    }
                },
                BusinessType = "individual",
                Email = shopOwnerEmailAddress,
                DefaultCurrency = "USD"
            };
            
            var service = new AccountService();
            var accountContract = await service.CreateAsync(options);

            var store = await platformDbContext.Stores.FirstAsync(x => x.ExternalId == storeExternalId);

            var storePaymentAccount = new StorePaymentAccount
            {
                AccountNumber = accountContract.Id,
                InsertedBy = shopOwnerEmailAddress
            };

            store.StorePaymentAccount = storePaymentAccount;
            await platformDbContext.SaveChangesAsync();
            
        }
    }
}