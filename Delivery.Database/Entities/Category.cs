using System.ComponentModel.DataAnnotations;

namespace Delivery.Database.Entities
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(300)]
        public string CategoryName { get; set; }

        [MaxLength(4000)]
        public string Description { get; set; }
        public int ParentCategoryId { get; set; }
        public int Order { get; set;  }
    }
}