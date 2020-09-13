using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Product.Domain.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Product.Domain.QueryHandlers
{
    public class ProductGetAllQueryHandler : IQueryHandler<ProductGetAllQuery, List<ProductContract>>
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly IMapper _mapper;

        public ProductGetAllQueryHandler(
            ApplicationDbContext appDbContext,
            IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }
        
        public Task<List<ProductContract>> Handle(ProductGetAllQuery query)
        {
            var result =  _appDbContext.Products.Include(x => x.Category).ToArrayAsync();

            return _mapper.Map<Task<List<ProductContract>>>(result);
        }
    }
}