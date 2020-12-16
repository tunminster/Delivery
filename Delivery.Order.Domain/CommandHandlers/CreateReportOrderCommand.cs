using System.Runtime.Serialization;
using Delivery.Order.Domain.Contracts.RestContracts;

namespace Delivery.Order.Domain.CommandHandlers
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