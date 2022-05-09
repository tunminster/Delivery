using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreTypeCreations;
using Delivery.Store.Domain.Converters.StoreTypeConverters;
using Delivery.Store.Domain.Handlers.CommandHandlers.StoreCreation;

namespace Delivery.Store.Domain.Handlers.CommandHandlers.StoreTypeCreation
{
    public class StoreTypeCreationCommandHandler :  ICommandHandler<StoreTypeCreationCommand, StoreTypeCreationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public StoreTypeCreationCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StoreTypeCreationStatusContract> HandleAsync(StoreTypeCreationCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var storeType = StoreTypeConverter.Convert(command.StoreTypeCreationContract);

            storeType.ExternalId = command.StoreTypeCreationStatusContract.StoreTypeId;
            storeType.InsertionDateTime = DateTimeOffset.UtcNow;
            storeType.InsertedBy = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail;
            storeType.IsDeleted = false;

            await databaseContext.StoreTypes.AddAsync(storeType);
            await databaseContext.SaveChangesAsync();
            
            return command.StoreTypeCreationStatusContract;
        }
    }
}