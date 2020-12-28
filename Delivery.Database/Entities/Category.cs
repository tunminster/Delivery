using System.ComponentModel.DataAnnotations;
using Delivery.Azure.Library.Database.Entities.V1;

namespace Delivery.Database.Entities
{
    public class Category : Entity
    {

        [MaxLength(300)]
        public string CategoryName { get; set; }

        [MaxLength(4000)]
        public string Description { get; set; }
        public int ParentCategoryId { get; set; }
        public int Order { get; set;  }
    }
}