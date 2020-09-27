using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Category.Domain.Contracts;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Category.Domain.QueryHandlers
{
    public class CategoryGetAllQueryHandler : IQueryHandler<CategoryGetAllQuery, List<CategoryContract>>
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly IMapper _mapper;

        public CategoryGetAllQueryHandler(ApplicationDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }
        public Task<List<CategoryContract>> Handle(CategoryGetAllQuery query)
        {
            var categoryContractList =  _appDbContext.Categories.Select(x => new CategoryContract()
            {
                Id = x.Id,
                CategoryName = x.CategoryName,
                Description = x.Description,
                Order = x.Order,
                ParentCategoryId = x.ParentCategoryId
            }).ToListAsync();
            
            return categoryContractList;
        }
    }
}