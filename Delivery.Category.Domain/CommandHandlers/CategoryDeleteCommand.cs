namespace Delivery.Category.Domain.CommandHandlers
{
    public class CategoryDeleteCommand
    {
        public CategoryDeleteCommand(string categoryId)
        {
            CategoryId = categoryId;
        }
        public string CategoryId { get; }
    }
}