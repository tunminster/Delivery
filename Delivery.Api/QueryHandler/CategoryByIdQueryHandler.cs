using System;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Api.Data;
using Delivery.Api.Domain.Query;
using Delivery.Api.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Api.QueryHandler
{
    public class CategoryByIdQueryHandler : IQueryHandler<CategoryByIdQuery, CategoryDto>
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

        public async Task<CategoryDto> Handle(CategoryByIdQuery query)
        {
            var result = await _appDbContext.Categories.FirstOrDefaultAsync(x => x.Id == query.CategoryId);
            return _mapper.Map<CategoryDto>(result);
        }
    }
}
