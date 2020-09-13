using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Order.Domain.Contracts;
using Delivery.Order.Domain.Contracts.RestContracts;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Order.Domain.QueryHandlers
{
    public class OrderByCustomerIdQueryHandler : IQueryHandler<OrderByCustomerIdQuery, List<OrderContract>>
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly IMapper _mapper;

        public OrderByCustomerIdQueryHandler(
            ApplicationDbContext appDbContext,
            IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }
        
        public Task<List<OrderContract>> Handle(OrderByCustomerIdQuery query)
        {
            var orderList =  _appDbContext.Orders.Where(x => x.CustomerId == query.CustomerId)
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product).ToListAsync();

            return _mapper.Map<Task<List<OrderContract>>>(orderList);
        }
    }
}