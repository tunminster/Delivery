using System.Runtime.Serialization;

namespace Delivery.Order.Domain.CommandHandlers
{
    [DataContract]
    public class CreateReportOrderCommand
    {
        [DataMember]
        public string Subject { get; set; }
        [DataMember]
        public string ContactNumber { get; set; }
        [DataMember]
        public string ReportCategory { get; set; }
        [DataMember]
        public int CustomerId { get; set; }
        [DataMember]
        public string Message { get; set; }
    }
}