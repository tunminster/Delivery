using AutoMapper;
using Delivery.Customer.Domain.Contracts;

namespace Delivery.Customer.Domain.Providers
{
    public class CustomerAutoMapperProvider : Profile
    {
        public CustomerAutoMapperProvider()
        {
            CreateMap<Database.Entities.Customer, CustomerContract>().ReverseMap();
        }
    }
}