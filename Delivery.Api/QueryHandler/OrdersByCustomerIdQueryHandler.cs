using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Api.Data;
using Delivery.Api.Domain.Query;
using Delivery.Api.Entities;
using Delivery.Api.Models.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Delivery.Api.QueryHandler
{
    public class OrdersByCustomerIdQueryHandler : IQueryHandler<GetOrderByCustomerIdQuery, OrderViewDto []>
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<OrdersByCustomerIdQueryHandler> _logger;

        public OrdersByCustomerIdQueryHandler(
            ApplicationDbContext appDbContext,
            IMapper mapper,
            ILogger<OrdersByCustomerIdQueryHandler> logger
            )
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OrderViewDto[]> Handle(GetOrderByCustomerIdQuery query)
        {
            var result  = _mapper.Map<OrderViewDto[]>(await _appDbContext.Orders.Where(x => x.CustomerId == query.CustomerId).Include(x => x.OrderItems).ToArrayAsync());
            

            return result;
        }
    }
}
