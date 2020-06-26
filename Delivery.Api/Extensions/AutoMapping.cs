using AutoMapper;
using Delivery.Api.Domain.Command;
using Delivery.Api.Entities;
using Delivery.Api.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Delivery.Api.Extensions
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<Customer, CustomerDto>().ReverseMap();
            CreateMap<Address, AddressDto>().ReverseMap();

            CreateMap<Category, CategoryDto>().ReverseMap();

            CreateMap<OrderItem, OrderItemDto>().ReverseMap();
            CreateMap<OrderItem, OrderViewItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
                .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.Product.UnitPrice));

            CreateMap<Order, OrderViewDto>().ReverseMap();

            CreateMap<ReportDto, CreateReportOrderCommand>()
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.UserId)).ReverseMap();

            CreateMap<CreateReportOrderCommand, Report>();

            /*CreateMap<Order, CreateOrderCommand>().ReverseMap();

            CreateMap<OrderItemDto, OrderItemCommand>().ReverseMap();
            CreateMap<OrderDto, CreateOrderCommand>()
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => Convert.ToDecimal(src.TotalAmount)))
                .ForMember(dest => dest.PaymentCard, opt => opt.MapFrom(src => src.CardNumber))
                .ForMember(dest => dest.PaymentCVC, opt => opt.MapFrom(src => src.Cvc))
                .ForMember(dest => dest.PaymentExpiryMonth, opt => opt.MapFrom(src => src.ExpiryMonth))
                .ForMember(dest => dest.PaymentExpiryYear, opt => opt.MapFrom(src => src.ExpiryYear));*/
                

        }
    }
}
