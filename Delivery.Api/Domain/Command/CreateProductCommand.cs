using System;
using Delivery.Api.Domain.Contract;
using Microsoft.AspNetCore.Http;

namespace Delivery.Api.Domain.Command
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
