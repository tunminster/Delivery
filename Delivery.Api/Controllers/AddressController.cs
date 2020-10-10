using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Address.Domain.CommandHandlers;
using Delivery.Address.Domain.Contracts;
using Delivery.Address.Domain.QueryHandlers;
using Delivery.Api.Models.Dto;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.QueryHandlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Delivery.Api.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly ICommandHandler<AddressCreationCommand, AddressCreationStatusContract> addressCreationCommandHandler;
        private readonly IQueryHandler<AddressByIdQuery, AddressContract> addressByIdQueryHandler;
        private readonly IQueryHandler<AddressByUserIdQuery, List<AddressContract>> addressByUserIdQueryHandler;

        public AddressController(
            ICommandHandler<AddressCreationCommand, AddressCreationStatusContract> addressCreationCommandHandler,
            IQueryHandler<AddressByIdQuery, AddressContract> addressByIdQueryHandler,
            IQueryHandler<AddressByUserIdQuery, List<AddressContract>> addressByUserIdQueryHandler)
        {
            this.addressCreationCommandHandler = addressCreationCommandHandler;
            this.addressByIdQueryHandler = addressByIdQueryHandler;
            this.addressByUserIdQueryHandler = addressByUserIdQueryHandler;
        }

        [HttpGet("GetAddressByUserId/{customerId}")]
        [ProducesResponseType(typeof(List<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAddressByUserId(int customerId,CancellationToken cancellationToken = default)
        {
            var addressByUserIdQuery = new AddressByUserIdQuery(customerId);
            var addressContactList = await addressByUserIdQueryHandler.Handle(addressByUserIdQuery);
            
            return Ok(addressContactList);
        }

        [HttpGet("GetAddressById/{id}")]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAddressById(int id)
        {
            var addressByIdQuery = new AddressByIdQuery(id);
            var addressContract = await addressByIdQueryHandler.Handle(addressByIdQuery);
            return Ok(addressContract);
        }

        [HttpPost("Create")]
        [ProducesResponseType(typeof(AddressCreationStatusContract), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddAddress(AddressContract addressContract)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var addressCreationCommand = new AddressCreationCommand(addressContract);
            var addressCreationStatusContract = await addressCreationCommandHandler.Handle(addressCreationCommand);

            return Ok(addressCreationStatusContract);
        }
    }
}
