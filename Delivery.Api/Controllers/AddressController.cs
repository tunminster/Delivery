using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Api.Data;
using Delivery.Api.Entities;
using Delivery.Api.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Delivery.Api.Extensions.HttpResults;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Delivery.Api.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly ILogger<AddressController> _logger;
        private readonly ApplicationDbContext _appDbContext;
        private readonly IMapper _mapper;

        public AddressController(ILogger<AddressController> logger,
            ApplicationDbContext appDbContext,
            IMapper mapper)
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        [HttpGet("GetAddressByUserId/{customerId}")]
        [ProducesResponseType(typeof(List<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAddressByUserId(int customerId,CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _appDbContext.Addresses.Where(x => x.CustomerId == customerId).ToListAsync(cancellationToken);
                var addressDtoList = _mapper.Map<List<AddressDto>>(result);
                return Ok(addressDtoList);
            }
            catch (Exception ex)
            {
                var errorMessage = "Fetching Address list";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }
        }

        [HttpGet("GetAddressById/{id}")]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAddressById(int id)
        {
            try
            {
                var result = await _appDbContext.Addresses.FirstOrDefaultAsync(x => x.Id == id);
                var addressDto = _mapper.Map<AddressDto>(result);
                return Ok(addressDto);
            }
            catch (Exception ex)
            {
                var errorMessage = "Fetching address by id";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }
        }

        [HttpPost("Create")]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddAddress(AddressDto addressDto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var address = _mapper.Map<Address>(addressDto);
                await _appDbContext.Addresses.AddAsync(address);
                await _appDbContext.SaveChangesAsync();

                addressDto.Id = address.Id;

                return CreatedAtAction(nameof(GetAddressById), new { id = address.Id }, addressDto);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error occurred in creating address";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }
        }


    }
}
