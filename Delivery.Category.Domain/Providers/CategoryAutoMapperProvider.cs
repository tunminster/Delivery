using AutoMapper;
using Delivery.Category.Domain.Contracts;
using Delivery.Category.Domain.Contracts.V1.ModelContracts;

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