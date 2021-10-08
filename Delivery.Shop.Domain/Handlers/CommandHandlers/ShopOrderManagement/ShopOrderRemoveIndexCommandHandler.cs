using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Constants;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopOrderManagement
{
    public record ShopOrderRemoveIndexCommand(string OrderId);
    
    public class ShopOrderRemoveIndexCommandHandler : ICommandHandler<ShopOrderRemoveIndexCommand, StatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public ShopOrderRemoveIndexCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StatusContract> Handle(ShopOrderRemoveIndexCommand command)
        {
            var elasticClient = serviceProvider.GetRequiredService<IElasticClient>();
            var indexExist = await elasticClient.Indices.ExistsAsync($"{ElasticSearchIndexConstants.ShopOrdersIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}");

            if (!indexExist.Exists) return new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow };
            var getResponse = await elasticClient.GetAsync<ShopOrderContract>(command.OrderId, d => d.Index($"{ElasticSearchIndexConstants.ShopOrdersIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}"));
            if (getResponse.Found)
            {
                await elasticClient.DeleteAsync<ShopOrderContract>(command.OrderId, d => d.Index($"{ElasticSearchIndexConstants.ShopOrdersIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}"));
            }
            return new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow };
        }
    }
}