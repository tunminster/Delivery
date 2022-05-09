using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Delivery.Store.Domain.ElasticSearch.Handlers.CommandHandlers.StoreIndexRemove
{
    public class StoreIndexRemoveCommandHandler : ICommandHandler<StoreIndexRemoveCommand, bool>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public StoreIndexRemoveCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<bool> HandleAsync(StoreIndexRemoveCommand command)
        {
            var elasticClient = serviceProvider.GetRequiredService<IElasticClient>();
            
            var deleteResponse = await elasticClient.DeleteAsync<StoreContract>(command.StoreDeletionContract.StoreId, d => d
                .Index(command.StoreDeletionContract.IndexName)
            );

            return deleteResponse.IsValid;
        }
    }
}