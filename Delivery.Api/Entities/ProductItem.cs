using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Delivery.Api.Entities
{
    public class ProductItem
    {
        [Key]
        public int Id { get; set; }
        public string ItemName { get; set; }
        public int ProductId { get; set; }
        public string UnitPrice { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
