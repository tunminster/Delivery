using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Address.Domain.CommandHandlers;
using Delivery.Address.Domain.Contracts;
using Delivery.Address.Domain.QueryHandlers;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Domain.FrameWork.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Customers
{
    /// <summary>
    ///  Customer address controller
    /// </summary>
    [Route("api/[controller]", Name = "12 - Customer Address management")]
    [PlatformSwaggerCategory(ApiCategory.Customer)]
    [ApiController]
    [Authorize]
    public class CustomerAddressController : Controller
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        ///  Address controller
        /// </summary>
        /// <param name="serviceProvider"></param>
        public CustomerAddressController(
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        ///  Get address by user id
        /// </summary>
        /// <returns></returns>
        [Route("GetAddress", Order = 1)]
        [HttpGet("GetAddressByUserId/{customerId}")]
        [ProducesResponseType(typeof(List<AddressContract>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAddressByUserIdAsync(int customerId,CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var addressByUserIdQuery = new AddressByUserIdQuery(customerId);
            var addressByUserIdQueryHandler = new AddressByUserIdQueryHandler(serviceProvider, executingRequestContextAdapter);
            
            var addressContactList = await addressByUserIdQueryHandler.Handle(addressByUserIdQuery);
            
            return Ok(addressContactList);
        }
        

        /// <summary>
        ///  Create address
        /// </summary>
        /// <returns></returns>
        [Route("Create", Order = 2)]
        [HttpPost]
        [ProducesResponseType(typeof(AddressCreationStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddAddressAsync(AddressContract addressContract)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var addressCreationCommand = new AddressCreationCommand(addressContract);
            var addressCreationCommandHandler =
                new AddressCreationCommandHandler(serviceProvider, executingRequestContextAdapter);
            
            var addressCreationStatusContract = await addressCreationCommandHandler.HandleAsync(addressCreationCommand);

            return Ok(addressCreationStatusContract);
        }
    }
}