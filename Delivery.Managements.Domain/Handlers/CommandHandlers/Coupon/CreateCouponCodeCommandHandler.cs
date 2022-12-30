using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Managements.Domain.Contracts.V1.RestContracts.Coupon;
using Delivery.Managements.Domain.Converters;

namespace Delivery.Managements.Domain.Handlers.CommandHandlers.Coupon
{
    public record CreateCouponCodeCommand(CouponManagementCreationContract CouponManagementCreationContract);
    public class CreateCouponCodeCommandHandler : ICommandHandler<CreateCouponCodeCommand, bool>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public CreateCouponCodeCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<bool> HandleAsync(CreateCouponCodeCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var couponCode = command.CouponManagementCreationContract.ConvertToCouponCode();

            databaseContext.Add(couponCode);
            return await databaseContext.SaveChangesAsync() > 0;
        }
    }
}