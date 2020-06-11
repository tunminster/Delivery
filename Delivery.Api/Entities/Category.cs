using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Delivery.Api.Entities
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(300)]
        public string CategoryName { get; set; }

        [Column(TypeName = "VARCHAR(4000)")]        
        public string Description { get; set; }
        public int ParentCategoryId { get; set; }
        public int Order { get; set;  }
    }
}
