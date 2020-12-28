using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Azure.Library.Database.Entities.V1;

namespace Delivery.Database.Entities
{
    public class Product : Entity
    {

        [MaxLength(300)]
        public string ProductName { get; set; }

        [MaxLength(4000)]
        public string Description { get; set; }
        public string ProductImage { get; set; }
        public string ProductImageUrl { get; set; }
        
        public int UnitPrice { get; set; }
        
        [MaxLength(50)]
        public string Currency { get; set; }
        [MaxLength(20)]
        public string CurrencySign { get; set; }
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }
        
    }
}