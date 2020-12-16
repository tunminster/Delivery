namespace Delivery.Category.Domain.CommandHandlers
{
    public class CategoryDeleteCommand
    {
        public CategoryDeleteCommand(int categoryId)
        {
            CategoryId = categoryId;
        }
        public int CategoryId { get; }
    }
}