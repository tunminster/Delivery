using System;
namespace Delivery.Api.Domain.Command
{
    public class CreateReportOrderCommand
    {
        public string Subject { get; set; }
        public string ContactNumber { get; set; }
        public string ReportCategory { get; set; }
        public string CustomerId { get; set; }
        public string Message { get; set; }
    }
}
