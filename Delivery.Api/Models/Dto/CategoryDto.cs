using System;
namespace Delivery.Api.Models.Dto
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public int ParentCategoryId { get; set; }
        public int Order { get; set; }
    }
}
