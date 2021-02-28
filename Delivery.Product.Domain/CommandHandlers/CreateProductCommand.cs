using Delivery.Product.Domain.Contracts;
using Delivery.Product.Domain.Contracts.V1.RestContracts;
using Microsoft.AspNetCore.Http;

namespace Delivery.Product.Domain.CommandHandlers
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