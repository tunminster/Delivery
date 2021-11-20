using Delivery.Order.Domain.Contracts.V1.RestContracts;

namespace Delivery.Order.Domain.Handlers.CommandHandlers
{
    public class CreateReportOrderCommand
    {
        public CreateReportOrderCommand(ReportCreationContract reportCreationContract)
        {
            ReportCreationContract = reportCreationContract;
        }
        
        public ReportCreationContract ReportCreationContract { get; }
    }
}