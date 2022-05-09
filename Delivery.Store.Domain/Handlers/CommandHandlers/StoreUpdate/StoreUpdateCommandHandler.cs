using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Domain.CommandHandlers;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreUpdate;
using Delivery.Store.Domain.Factories.GeoLocationFactory;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Store.Domain.Handlers.CommandHandlers.StoreUpdate
{
    public class StoreUpdateCommandHandler : ICommandHandler<StoreUpdateCommand, StoreUpdateStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public StoreUpdateCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StoreUpdateStatusContract> HandleAsync(StoreUpdateCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var store = await databaseContext.Stores.FirstOrDefaultAsync(x =>
                x.ExternalId == command.StoreUpdateContract.StoreId);

            var storeType =
                await databaseContext.StoreTypes.FirstOrDefaultAsync(x =>
                    x.ExternalId == command.StoreUpdateContract.StoreTypeId);

            var storePaymentAccount = await databaseContext.StorePaymentAccounts
                .FirstOrDefaultAsync(x => x.Id == store.StorePaymentAccountId);

            if (storePaymentAccount != null)
            {
                storePaymentAccount.AccountNumber = command.StoreUpdateContract.PaymentAccountNumber;
            }
            else
            {
                store.StorePaymentAccount = new StorePaymentAccount
                {
                    AccountNumber = command.StoreUpdateContract.PaymentAccountNumber,
                    IsDeleted = false
                };
            }
            
            store.StoreName = command.StoreUpdateContract.StoreName;
            store.AddressLine1 = command.StoreUpdateContract.AddressLine1;
            store.AddressLine2 = command.StoreUpdateContract.AddressLine2;
            store.ImageUri = string.IsNullOrEmpty(command.StoreUpdateContract.ImageUri) ? store.ImageUri : command.StoreUpdateContract.ImageUri;
            store.City = command.StoreUpdateContract.City;
            store.County = command.StoreUpdateContract.County;
            store.Country = command.StoreUpdateContract.Country;
            store.PostalCode = command.StoreUpdateContract.PostalCode;
            store.StoreTypeId = storeType.Id;
            store.IsActive = command.StoreUpdateContract.IsActive;

            if (store.OpeningHours == null || store.OpeningHours.Count < 1)
            {
                foreach (var storeOpeningHour in command.StoreUpdateContract.StoreOpeningHours)
                {
                    var openingHour = await databaseContext.OpeningHours.FirstOrDefaultAsync(x =>
                        x.StoreId == store.Id && x.DayOfWeek == storeOpeningHour.DayOfWeek);

                    if (openingHour != null)
                    {
                        openingHour.Open = storeOpeningHour.Open;
                        openingHour.Close = storeOpeningHour.Close;

                        databaseContext.OpeningHours.Update(openingHour);
                    }
                    else
                    {
                        databaseContext.OpeningHours.Add(new OpeningHour
                        {
                            DayOfWeek = storeOpeningHour.DayOfWeek,
                            StoreId = store.Id,
                            Open = storeOpeningHour.Open,
                            Close = storeOpeningHour.Close,
                            IsDeleted = false,
                            InsertedBy = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail,
                            InsertionDateTime = DateTimeOffset.UtcNow
                        });
                    }
                    
                }
            }
            else
            {
                foreach (var storeOpeningHour in command.StoreUpdateContract.StoreOpeningHours)
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
            }
            
            databaseContext.Stores.Update(store);
            await databaseContext.SaveChangesAsync();
            
            await new StoreGeoLocationFactory(serviceProvider, executingRequestContextAdapter)
                .PublishStoreGeoUpdateMessageAsync(store.ExternalId);

            command.StoreUpdateStatusContract.StoreId = store.ExternalId;
            command.StoreUpdateStatusContract.InsertionDateTime = DateTimeOffset.UtcNow;

            return command.StoreUpdateStatusContract;
        }
    }
}