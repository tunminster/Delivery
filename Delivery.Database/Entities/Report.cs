using System.ComponentModel.DataAnnotations;
using Delivery.Azure.Library.Database.Entities.V1;

namespace Delivery.Database.Entities
{
    public class Report : Entity
    {

        public int CustomerId { get; set; }

        [MaxLength(250)]
        public string Subject { get; set; }

        [MaxLength(20)]
        public string ContactNumber { get; set; }

        [MaxLength(20)]
        public string ReportCategory { get; set; }

        [MaxLength(500)]
        public string Message { get; set; }
    }
}