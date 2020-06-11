﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Delivery.Api.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(300)]
        public string ProductName { get; set; }

        [MaxLength(4000)]
        public string Description { get; set; }
        public string ProductImage { get; set; }
        public string ProductImageUrl { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        [MaxLength(50)]
        public string Currency { get; set; }
        [MaxLength(20)]
        public string CurrencySign { get; set; }
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }
        
    }
}
