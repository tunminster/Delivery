using System;
using System.Collections.Generic;

namespace Delivery.Api.Models.Dto
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public List<AddressDto> Addresses { get; set; }
    }
}
