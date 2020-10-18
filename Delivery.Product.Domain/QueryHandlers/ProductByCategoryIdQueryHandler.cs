using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Product.Domain.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Product.Domain.QueryHandlers
{
    public class ProductByCategoryIdQueryHandler : IQueryHandler<ProductByCategoryIdQuery, List<ProductContract>>
    {
        private readonly ApplicationDbContext appDbContext;
        private readonly IMapper mapper;

        public ProductByCategoryIdQueryHandler(ApplicationDbContext appDbContext, IMapper mapper)
        {
            this.appDbContext = appDbContext;
            this.mapper = mapper;
        }
        
        public async Task<List<ProductContract>> Handle(ProductByCategoryIdQuery query)
        {
            var productList = await appDbContext.Products.Where(x => x.CategoryId == query.CategoryId).ToListAsync();
            var productContractList = mapper.Map<List<ProductContract>>(productList);
            return productContractList;
        }
    }
}