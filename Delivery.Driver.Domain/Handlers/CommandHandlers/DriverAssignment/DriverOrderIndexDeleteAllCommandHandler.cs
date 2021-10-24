using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Constants;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverAssignment
{
    public record DriverOrderIndexDeleteAllCommand(DateTimeOffset DateCreated);
    public class DriverOrderIndexDeleteAllCommandHandler : ICommandHandler<DriverOrderIndexDeleteAllCommand>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public DriverOrderIndexDeleteAllCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task Handle(DriverOrderIndexDeleteAllCommand command)
        {
            var elasticClient = serviceProvider.GetRequiredService<IElasticClient>();
            var indexExist = await elasticClient.Indices.ExistsAsync($"{ElasticSearchIndexConstants.DriverOrdersIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}");

            if (indexExist.Exists)
            {
                await elasticClient.Indices.DeleteAsync($"{ElasticSearchIndexConstants.DriverOrdersIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}");
            }
        }
    }
}