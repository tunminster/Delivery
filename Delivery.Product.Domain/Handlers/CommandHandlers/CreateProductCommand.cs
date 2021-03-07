using Delivery.Product.Domain.Contracts.V1.RestContracts;

namespace Delivery.Product.Domain.Handlers.CommandHandlers
{
    public class CreateProductCommand
    {
        public CreateProductCommand(ProductCreationContract productCreationContract, string productId)
        {
            ProductCreationContract = productCreationContract;
            ProductId = productId;
        }
        public ProductCreationContract ProductCreationContract { get; }
        
        public string ProductId { get; }
    }
}