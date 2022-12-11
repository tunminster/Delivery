using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Kernel;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Enums;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverReAssignment;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Strategies.DriverAssignmentStrategy
{
    public class DriverReAssignmentDispatcher
    {
        private readonly List<IDriverReAssignmentStrategy> strategies = new();

        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public DriverReAssignmentDispatcher(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
            strategies.Add(new ReAssignDeliveryOrderStrategy(serviceProvider, executingRequestContextAdapter));
            strategies.Add(new AwaitingDeliveryOrderStrategy(serviceProvider, executingRequestContextAdapter));
        }

        public async Task<DriverReAssignmentCreationStatusContract> DispatchAsync(int driverOrderId,
            int awaitingMinutes, DriverOrderStatus driverOrderStatus)
        {
            var driverReAssignmentStrategies = strategies.Where(x => x.AppliesTo(driverOrderStatus)).ToList();

            if (!driverReAssignmentStrategies.Any())
            {
                serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                    .TrackTrace(
                        $"No {nameof(IDriverReAssignmentStrategy)} is configured for {nameof(DriverOrderStatus)}:{driverOrderStatus}",
                        SeverityLevel.Critical, executingRequestContextAdapter.GetTelemetryProperties());
            }

            if (driverReAssignmentStrategies.Count > 1)
            {
                throw new InvalidOperationException(
                    $"Multiple {nameof(IDriverReAssignmentStrategy)} is not allowed for {nameof(DriverOrderStatus)}:{driverOrderStatus}");
            }

            return await driverReAssignmentStrategies.Single().ExecuteAsync(driverOrderId, awaitingMinutes);
        }
    }
}