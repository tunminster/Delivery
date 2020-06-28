using System;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Api.Data;
using Delivery.Api.Domain.Query;
using Delivery.Api.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Api.QueryHandler
{
    public class ProductGetAllQueryHandler : IQueryHandler<ProductGetAllQuery, ProductDto[]>
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

        public async Task<ProductDto[]> Handle(ProductGetAllQuery query)
        {
            var result = await _appDbContext.Products.Include(x => x.Category).ToArrayAsync();

            return _mapper.Map<ProductDto[]>(result);
        }
    }
}
