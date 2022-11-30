using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Managements.Domain.Contracts.V1.MessageContracts.MeatOptions;
using Delivery.Managements.Domain.Contracts.V1.RestContracts.MeatOptions;
using Delivery.Managements.Domain.Converters;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Managements.Domain.Handlers.CommandHandlers
{
    public record MeatOptionCreationCommand(MeatOptionCreationMessageContract MeatOptionCreationContract);

    public class
        MeatOptionCreationCommandHandler : ICommandHandler<MeatOptionCreationCommand, MeatOptionCreationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;

        public MeatOptionCreationCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        public async Task<MeatOptionCreationStatusContract> HandleAsync(MeatOptionCreationCommand command)
        {
            await using var databaseContext =
                await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var product =
                await databaseContext.Products.SingleAsync(x =>
                    x.ExternalId == command.MeatOptionCreationContract.ProductId);

            var meatOptionEntity = command.MeatOptionCreationContract.ConvertMeatOptionToEntity(product.Id);

            await databaseContext.MeatOptions.AddAsync(meatOptionEntity);
            await databaseContext.SaveChangesAsync();

            var meatOptionCreationStatusContract = new MeatOptionCreationStatusContract
            {
                MeatOptionId = meatOptionEntity.ExternalId
            };

            return meatOptionCreationStatusContract;
        }
    }
}