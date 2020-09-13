using AutoMapper;
using Delivery.Database.Entities;
using Delivery.Order.Domain.Contracts;
using Delivery.Order.Domain.Contracts.RestContracts;

namespace Delivery.Order.Domain.Providers
{
    public class OrderAutoMapperProvider : Profile
    {
        public OrderAutoMapperProvider()
        {
            CreateMap<OrderItem, OrderItemContract>().ReverseMap();
            CreateMap<Database.Entities.Order, OrderContract>().ReverseMap();
        }
    }
}