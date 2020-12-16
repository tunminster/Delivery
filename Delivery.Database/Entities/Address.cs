using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Delivery.Database.Entities
{
    public class Address
    {
        [Key]
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string AddressLine { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }
        public string Lat { get; set; }
        public string Lng { get; set; }
        public string Country { get; set; }
        public bool Disabled { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }
            
    }
}