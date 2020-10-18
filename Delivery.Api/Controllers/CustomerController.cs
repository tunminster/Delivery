using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Customer.Domain.Contracts;
using Delivery.Customer.Domain.QueryHandlers;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
        private readonly IQueryHandler<CustomerByUsernameQuery, CustomerContract> queryCustomerByUsernameQuery;

        public CustomerController(IQueryHandler<CustomerByUsernameQuery, CustomerContract> queryCustomerByUsernameQuery)
        {
            this.queryCustomerByUsernameQuery = queryCustomerByUsernameQuery;
        }

        [HttpGet("GetCustomer")]
        [Authorize]
        [ProducesResponseType(typeof(CustomerContract), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCustomer()
        {
            string userName = HttpContext?.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var customerByUsernameQuery = new CustomerByUsernameQuery(userName);
            var customerContract = await queryCustomerByUsernameQuery.Handle(customerByUsernameQuery);
            
            return Ok(customerContract);
        }
    }
}
