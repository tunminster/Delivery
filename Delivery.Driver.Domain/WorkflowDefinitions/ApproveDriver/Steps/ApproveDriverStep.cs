using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Microservices.Hosting.Workflows;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.Extensions;
using Delivery.Database.Context;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverApproval;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverApproval;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkflowCore.Interface;

namespace Delivery.Driver.Domain.WorkflowDefinitions.ApproveDriver.Steps;

public record ApproveDriverStepCommand(DriverApprovalContract DriverApprovalContract);

public record ApproveDriverStepResult(DriverApprovalStatusContract DriverApprovalStatusContract);

public class ApproveDriverStep : StepStatefulContextBodyAsync<ApproveDriverStepCommand, ApproveDriverStepResult>
{
    public ApproveDriverStep(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<ApproveDriverStepResult> ExecuteStepAsync(IStepExecutionContext context)
    {
        await using var databaseContext = await PlatformDbContext.CreateAsync(ServiceProvider, ExecutingRequestContext);
        var driver = await databaseContext.Drivers.Where(x => x.EmailAddress == Input.Data.DriverApprovalContract.Username).FirstOrDefaultAsync();
            
        ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
            .TrackTrace($"{nameof(DriverApprovalCommandHandler)} executed. command: {Input.Data.DriverApprovalContract.ConvertToJson()}. driver: {driver.ConvertToJson()}", SeverityLevel.Information, ExecutingRequestContext.GetTelemetryProperties());

        if (driver != null)
        {
            driver.Approved = Input.Data.DriverApprovalContract.Approve;

            await databaseContext.SaveChangesAsync();
        }

        var driverApprovalStatusContract = new DriverApprovalStatusContract
        {
            DateCreated = DateTimeOffset.UtcNow,
            DriverName = driver?.FullName ?? string.Empty,
            EmailAddress = driver?.EmailAddress ?? string.Empty
        };

        return new ApproveDriverStepResult(driverApprovalStatusContract);
    }
}