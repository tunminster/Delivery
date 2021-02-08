using Delivery.Product.Domain.Contracts;
using Delivery.Product.Domain.Contracts.V1.RestContracts;
using Microsoft.AspNetCore.Http;

namespace Delivery.Product.Domain.CommandHandlers
{
    public class CreateProductCommand
    {
        public CreateProductCommand(ProductCreationContract productCreationContract, IFormFile file)
        {
            ProductCreationContract = productCreationContract;
            File = file;
        }
        public ProductCreationContract ProductCreationContract { get; }
        
        public IFormFile File { get; }
    }
}