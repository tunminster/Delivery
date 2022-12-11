using System.Threading.Tasks;
using Delivery.Database.Enums;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverReAssignment;

namespace Delivery.Driver.Domain.Strategies.DriverAssignmentStrategy
{
    public interface IDriverReAssignmentStrategy
    {
        bool AppliesTo(DriverOrderStatus driverOrderStatus);

        Task<DriverReAssignmentCreationStatusContract> ExecuteAsync(int driverOrderId, int awaitingMinutes);
    }
}