using System.Collections.Generic;
using Delivery.Address.Domain.Contracts;
using Delivery.Domain.QueryHandlers;

namespace Delivery.Address.Domain.QueryHandlers
{
    public class AddressByUserIdQuery : IQuery<List<AddressContract>>
    {
        public AddressByUserIdQuery(int userId)
        {
            UserId = userId;
        }
        public int UserId { get; }
    }
}