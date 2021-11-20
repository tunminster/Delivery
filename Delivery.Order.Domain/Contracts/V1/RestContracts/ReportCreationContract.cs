using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Order.Domain.Contracts.V1.RestContracts
{
    [DataContract]
    public class ReportCreationContract
    {
        [DataMember]
        [MaxLength(250)]
        public string Subject { get; set; }
        
        [DataMember]
        public string ContactNumber { get; set; }
        
        [DataMember]
        public string ReportCategory { get; set; }
        
        [DataMember]
        public int UserId { get; set; }
        
        [DataMember]
        public string Message { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(Subject)}: {Subject.Format()}," +
                   $"{nameof(ContactNumber)}: {ContactNumber.Format()}," +
                   $"{nameof(ReportCategory)}: {ReportCategory.Format()}," +
                   $"{nameof(UserId)}: {UserId.Format()}," +
                   $"{nameof(Message)}: {Message.Format()};";

        }
    }
}