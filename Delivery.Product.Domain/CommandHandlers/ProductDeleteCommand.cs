namespace Delivery.Product.Domain.CommandHandlers
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