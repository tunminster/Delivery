using System.Collections.Generic;
using Delivery.Address.Domain.Contracts;
using Delivery.Domain.QueryHandlers;

namespace Delivery.Address.Domain.QueryHandlers
{
    public class AddressByIdQuery : IQuery<AddressContract>
    {
        public AddressByIdQuery(int addressId)
        {
            AddressId = addressId;
        }
        public int AddressId { get; }
    }
}