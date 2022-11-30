using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Store.Domain.Handlers.CommandHandlers.StoreDelete
{
    public record StoreDeleteCommand(string StoreId);
    
    public class StoreDeleteCommandHandler : ICommandHandler<StoreDeleteCommand>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public StoreDeleteCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task HandleAsync(StoreDeleteCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var store = await databaseContext.Stores.SingleOrDefaultAsync(x => x.ExternalId == command.StoreId);

            if (store == null)
            {
                throw new InvalidOperationException($"Expected to be found a store by {command.StoreId}").WithTelemetry(
                    executingRequestContextAdapter.GetTelemetryProperties());
            }

            store.IsDeleted = true;
            store.IsActive = false;

            await databaseContext.SaveChangesAsync();

        }
    }
}