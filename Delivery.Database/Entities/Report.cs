using System.ComponentModel.DataAnnotations;

namespace Delivery.Database.Entities
{
    public class Report
    {
        [Key]
        public int Id { get; set; }

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