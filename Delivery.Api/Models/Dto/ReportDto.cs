using System;
using System.ComponentModel.DataAnnotations;

namespace Delivery.Api.Models.Dto
{
    public class ReportDto
    {
        [MaxLength(250)]
        public string Subject { get; set; }
        public string ContactNumber { get; set; }
        public string ReportCategory { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
    }
}
