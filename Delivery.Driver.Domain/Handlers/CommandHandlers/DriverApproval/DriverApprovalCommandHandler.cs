using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverApproval;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverApproval
{
    public record DriverApprovalCommand(DriverApprovalContract DriverApprovalContract);
    public class DriverApprovalCommandHandler : ICommandHandler<DriverApprovalCommand, DriverApprovalStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        public DriverApprovalCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<DriverApprovalStatusContract> HandleAsync(DriverApprovalCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var driver = await databaseContext.Drivers.Where(x => x.EmailAddress.ToLowerInvariant() == command.DriverApprovalContract.Username.ToLowerInvariant()).FirstOrDefaultAsync();
            
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                .TrackTrace($"{nameof(DriverApprovalCommandHandler)} executed. command: {command.ConvertToJson()}. driver: {driver.ConvertToJson()}", SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());

            if (driver != null)
            {
                driver.Approved = command.DriverApprovalContract.Approve;

                await databaseContext.SaveChangesAsync();
            }

            var driverApprovalStatusContract = new DriverApprovalStatusContract
            {
                DateCreated = DateTimeOffset.UtcNow
            };

            return driverApprovalStatusContract;
        }
    }
}