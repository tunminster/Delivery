using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Delivery.Api.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public string ProductImageUrl { get; set; }
        public string UnitPrice { get; set; }
        public int CategoryId { get; set; }
        
    }
}
