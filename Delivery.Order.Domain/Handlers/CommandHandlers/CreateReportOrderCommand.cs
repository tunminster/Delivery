using Delivery.Order.Domain.Contracts.RestContracts;

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