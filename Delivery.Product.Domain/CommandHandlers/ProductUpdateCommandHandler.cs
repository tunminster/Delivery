using System.Threading.Tasks;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Product.Domain.CommandHandlers
{
    public class ProductUpdateCommandHandler : ICommandHandler<ProductUpdateCommand, bool>
    {
        private readonly ApplicationDbContext appDbContext;
        private readonly Delivery.Domain.Configuration.AzureStorageConfig storageConfig;

        public ProductUpdateCommandHandler(ApplicationDbContext appDbContext, AzureStorageConfig storageConfig)
        {
            this.appDbContext = appDbContext;
            this.storageConfig = storageConfig;
        }
        
        public async Task<bool> Handle(ProductUpdateCommand command)
        {
            var product = await appDbContext.Products.FindAsync(command.ProductContract.Id);
            if (product == null)
            {
                return false;
            }

            product.CategoryId = command.ProductContract.CategoryId;
            product.Description = command.ProductContract.Description;
            product.UnitPrice = command.ProductContract.UnitPrice;
            product.ProductImage = command.ProductContract.ProductImage;
            product.ProductImageUrl = command.ProductContract.ProductImageUrl;

            appDbContext.Entry(product).State = EntityState.Modified;
            await appDbContext.SaveChangesAsync();

            return true;
        }
    }
}