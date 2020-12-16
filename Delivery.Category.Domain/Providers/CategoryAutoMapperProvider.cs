using AutoMapper;
using Delivery.Category.Domain.Contracts;

namespace Delivery.Category.Domain.Providers
{
    public class CategoryAutoMapperProvider : Profile
    {
        public CategoryAutoMapperProvider()
        {
            CreateMap<Database.Entities.Category, CategoryContract>().ReverseMap();
        }
    }
}