using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Enums;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverReAssignment;

namespace Delivery.Driver.Domain.Strategies.DriverAssignmentStrategy
{
    public abstract class DriverReAssignmentStrategy : IDriverReAssignmentStrategy
    {
        protected readonly IServiceProvider ServiceProvider;
        protected readonly IExecutingRequestContextAdapter ExecutingRequestContextAdapter;

        protected DriverReAssignmentStrategy(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            ServiceProvider = serviceProvider;
            ExecutingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public abstract bool AppliesTo(DriverOrderStatus driverOrderStatus);


        public abstract Task<DriverReAssignmentCreationStatusContract> ExecuteAsync(int driverOrderId, int awaitingMinutes);

    }
}