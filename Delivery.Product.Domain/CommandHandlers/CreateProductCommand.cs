using Delivery.Product.Domain.Contracts;
using Microsoft.AspNetCore.Http;

namespace Delivery.Product.Domain.CommandHandlers
{
    public class CreateProductCommand
    {
        public CreateProductCommand(ProductContract productContract, IFormFile file)
        {
            ProductContract = productContract;
            File = file;
        }
        public ProductContract ProductContract { get; }
        
        public IFormFile File { get; }
    }
}