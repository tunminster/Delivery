using System.Threading.Tasks;
using AutoMapper;
using Delivery.Category.Domain.Contracts;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Category.Domain.QueryHandlers
{
    public class CategoryByIdQueryHandler: IQueryHandler<CategoryByIdQuery, CategoryContract>
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly IMapper _mapper;
        
        public CategoryByIdQueryHandler(
            ApplicationDbContext appDbContext,
            IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }
        
        public Task<CategoryContract> Handle(CategoryByIdQuery query)
        {
            var category =  _appDbContext.Categories.FirstOrDefaultAsync(x => x.Id == query.CategoryId);
            
            return _mapper.Map<Task<CategoryContract>>(category);
        }
    }
}