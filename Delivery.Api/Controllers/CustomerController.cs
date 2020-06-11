using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Api.Data;
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
    public class CustomerController : ControllerBase
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly ApplicationDbContext _appDbContext;
        private readonly IMapper _mapper;

        public CustomerController(ILogger<CustomerController> logger,
        ApplicationDbContext appDbContext,
        IMapper mapper
        )
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        [HttpGet("GetCustomer")]
        [Authorize]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCustomer()
        {
            try
            {
                string userName = HttpContext?.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value;
                var result = await _appDbContext.Customers.Where(x => x.Username.ToLower() == userName.ToLower()).FirstOrDefaultAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error occurred in getting User details";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }
        }
    }
}
