using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Managements.Domain.Contracts.V1.RestContracts.MeatOptionValues;
using Delivery.Managements.Domain.Converters;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Managements.Domain.Handlers.CommandHandlers
{
    public record MeatOptionValueCreationCommand(MeatOptionValueCreationContract MeatOptionValueCreationContract);
    public class MeatOptionValueCreationCommandHandler : ICommandHandler<MeatOptionValueCreationCommand>
    {
        private readonly IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;

        public MeatOptionValueCreationCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task HandleAsync(MeatOptionValueCreationCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var meatOption =
                await databaseContext.MeatOptions.SingleAsync(x => x.ExternalId == command.MeatOptionValueCreationContract.MeatOptionId);
            
            var meatOptionValueEntity = command.MeatOptionValueCreationContract.ConvertMeatOptionValueToEntity(meatOption.Id);
            await databaseContext.MeatOptionValues.AddAsync(meatOptionValueEntity);
            await databaseContext.SaveChangesAsync();
        }
    }
}