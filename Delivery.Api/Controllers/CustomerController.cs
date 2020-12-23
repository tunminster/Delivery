using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Customer.Domain.Contracts;
using Delivery.Customer.Domain.QueryHandlers;
using Delivery.Database.Context;
using Delivery.Domain.FrameWork.Context;
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
        //private readonly IQueryHandler<CustomerByUsernameQuery, CustomerContract> queryCustomerByUsernameQuery;
        private readonly IServiceProvider serviceProvider;

        public CustomerController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [HttpGet("GetCustomer")]
        [Authorize]
        [ProducesResponseType(typeof(CustomerContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetCustomerAsync()
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            string userName = HttpContext?.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var customerByUsernameQuery = new CustomerByUsernameQuery(userName);

            var queryCustomerByUsernameQuery =
                new CustomerByUsernameQueryHandler(serviceProvider, executingRequestContextAdapter);
            var customerContract = await queryCustomerByUsernameQuery.Handle(customerByUsernameQuery);
            
            return Ok(customerContract);
        }
    }
}
