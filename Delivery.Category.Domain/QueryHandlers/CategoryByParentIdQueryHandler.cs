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
    public class CategoryByParentIdQueryHandler :IQueryHandler<CategoryByParentIdQuery, List<CategoryContract>>
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly IMapper _mapper;

        public CategoryByParentIdQueryHandler(ApplicationDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }
        
        public Task<List<CategoryContract>> Handle(CategoryByParentIdQuery query)
        {
            var result = _appDbContext.Categories
                .Where(x => x.ParentCategoryId == query.ParentId)
                .Select(x => new CategoryContract()
                {
                    Id = x.Id,
                    CategoryName = x.CategoryName,
                    Description = x.Description,
                    Order = x.Order,
                    ParentCategoryId = x.ParentCategoryId
                })
                .ToListAsync();

            return result;
        }
    }
}