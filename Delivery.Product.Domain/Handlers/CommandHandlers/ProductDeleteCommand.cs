namespace Delivery.Product.Domain.Handlers.CommandHandlers
{
    public class ProductDeleteCommand
    {
        public ProductDeleteCommand(int productId)
        {
            ProductId = productId;
        }
        public int ProductId { get; }
    }
}