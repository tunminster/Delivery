using System.Threading.Tasks;
using AutoMapper;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Product.Domain.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Product.Domain.QueryHandlers
{
    public class ProductByIdQueryHandler : IQueryHandler<ProductByIdQuery, ProductContract>
    {
        private readonly ApplicationDbContext appDbContext;
        private readonly IMapper mapper;

        public ProductByIdQueryHandler(ApplicationDbContext appDbContext, IMapper mapper)
        {
            this.appDbContext = appDbContext;
            this.mapper = mapper;
        }
        
        public async Task<ProductContract> Handle(ProductByIdQuery query)
        {
            var product = await appDbContext.Products.FirstOrDefaultAsync(x => x.Id == query.ProductId);
            var productContract = mapper.Map<ProductContract>(product);
            return productContract;
        }
    }
}