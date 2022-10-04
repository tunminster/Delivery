using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Address.Domain.CommandHandlers;
using Delivery.Address.Domain.Contracts;
using Delivery.Address.Domain.QueryHandlers;
using Delivery.Api.Models.Dto;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.FrameWork.Context;
using Delivery.Domain.QueryHandlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Delivery.Api.Controllers
{
    
    /// <summary>
    ///  Address controller
    /// </summary>
    [Route("api/[controller]", Name = "2 - Address management")]
    [PlatformSwaggerCategory(ApiCategory.Management)]
    [ApiController]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        ///  Address controller
        /// </summary>
        /// <param name="serviceProvider"></param>
        public AddressController(
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        ///  Get address by user id
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("GetAddressByUserId/{customerId}")]
        [ProducesResponseType(typeof(List<AddressContract>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAddressByUserId(int customerId,CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var addressByUserIdQuery = new AddressByUserIdQuery(customerId);
            var addressByUserIdQueryHandler = new AddressByUserIdQueryHandler(serviceProvider, executingRequestContextAdapter);
            
            var addressContactList = await addressByUserIdQueryHandler.Handle(addressByUserIdQuery);
            
            return Ok(addressContactList);
        }

        /// <summary>
        ///  Get address by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("GetAddressById/{id}", Order = 1)]
        [HttpGet]
        [ProducesResponseType(typeof(AddressContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAddressByIdAsync(int id)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var addressByIdQuery = new AddressByIdQuery(id);
            var addressByIdQueryHandler = new AddressByIdQueryHandler(serviceProvider, executingRequestContextAdapter);
            
            var addressContract = await addressByIdQueryHandler.Handle(addressByIdQuery);
            return Ok(addressContract);
        }

        /// <summary>
        ///  Create address
        /// </summary>
        /// <param name="addressContract"></param>
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
