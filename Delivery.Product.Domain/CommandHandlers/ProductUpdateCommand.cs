using System;
using Delivery.Product.Domain.Contracts;
using Delivery.Product.Domain.Contracts.V1.ModelContracts;
using Microsoft.AspNetCore.Http;

namespace Delivery.Product.Domain.CommandHandlers
{
    public class ProductUpdateCommand
    {
        public ProductUpdateCommand(ProductContract productContract, IFormFile file)
        {
            ProductContract = productContract;
            File = file;
        }
        
        public ProductContract ProductContract { get; }
        public IFormFile File { get; }
    }
}