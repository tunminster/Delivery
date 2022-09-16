using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverProfile
{
    public record DriverProfileBankDetailsCommand(DriverProfileBankDetailsContract DriverProfileBankDetailsContract);
    public class DriverProfileBankDetailsCommandHandler : ICommandHandler<DriverProfileBankDetailsCommand>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        public DriverProfileBankDetailsCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task Handle(DriverProfileBankDetailsCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected an authenticated user");
            var driver = await databaseContext.Drivers.SingleOrDefaultAsync(x =>
                x.ExternalId == command.DriverProfileBankDetailsContract.DriverId && x.EmailAddress == userEmail);

            driver.BankName = command.DriverProfileBankDetailsContract.BankName;
            driver.BankAccountNumber = command.DriverProfileBankDetailsContract.AccountNumber;
            driver.RoutingNumber = command.DriverProfileBankDetailsContract.RoutingNumber;
            driver.IsBankDetailsUpdated = true;
            
            await databaseContext.SaveChangesAsync();
        }
    }
}