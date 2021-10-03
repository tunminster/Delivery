using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile;

namespace Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverProfile
{
    public class DriverServiceAreaUpdateMessageContract : AuditableResponseMessage<DriverServiceAreaUpdateContract, StatusContract>
    {
    }
}