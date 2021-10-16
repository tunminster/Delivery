using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Managements.Domain.Contracts.V1.RestContracts;

namespace Delivery.Managements.Domain.Handlers.CommandHandlers
{
    public record ManagementUserCreationCommand(ManagementUserCreationContract ManagementUserCreationContract);
    
    public class ManagementUserCreationCommandHandler : ICommandHandler<ManagementUserCreationCommand>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public ManagementUserCreationCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task Handle(ManagementUserCreationCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            throw new System.NotImplementedException();
        }
    }
}