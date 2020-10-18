using System.Threading.Tasks;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;

namespace Delivery.Product.Domain.CommandHandlers
{
    public class ProductDeleteCommandHandler : ICommandHandler<ProductDeleteCommand, bool>
    {
        private readonly ApplicationDbContext appDbContext;

        public ProductDeleteCommandHandler(ApplicationDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }
        
        public async Task<bool> Handle(ProductDeleteCommand command)
        {
            var product = await appDbContext.Products.FindAsync(command.ProductId);
            if (product == null)
            {
                return false;
            }
            
            appDbContext.Products.Remove(product);
            await appDbContext.SaveChangesAsync();
            return true;
        }
    }
}