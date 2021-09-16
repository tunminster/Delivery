using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopOrderManagement
{
    public record ShopOrderDriverRequestCommand(ShopOrderDriverRequestContract ShopOrderDriverRequestContract);
    
    public class ShopOrderDriverRequestCommandHandler : ICommandHandler<ShopOrderDriverRequestCommand, StatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public ShopOrderDriverRequestCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public Task<StatusContract> Handle(ShopOrderDriverRequestCommand command)
        {
            throw new System.NotImplementedException();
        }
    }
}