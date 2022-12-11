using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverReAssignment;

namespace Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverAssignment;

public class DriverReAssignmentMessage : AuditableRequestMessage<DriverReAssignmentCreationContract>
{
    
}