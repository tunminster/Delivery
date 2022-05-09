using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Customer.Domain.Contracts;
using Delivery.Customer.Domain.Contracts.V1.RestContracts;
using Delivery.Customer.Domain.Handlers.CommandHandlers;
using Delivery.Customer.Domain.Handlers.QueryHandlers;
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
    /// <summary>
    ///  Customer controller
    /// </summary>
    [Route("api/[controller]", Name = "4 - Customer")]
    [PlatformSwaggerCategory(ApiCategory.Customer)]
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        public CustomerController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        ///  Get customer
        /// </summary>
        /// <returns></returns>
        [Route("GetCustomer", Order = 1)]
        [HttpGet]
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
        
        /// <summary>
        ///  Update customer
        /// </summary>
        /// <param name="customerUpdateContract"></param>
        /// <returns></returns>
        [Route("Update-Customer", Order = 2)]
        [HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(CustomerUpdateStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateCustomerAsync(CustomerUpdateContract customerUpdateContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var updateCustomerCommand = new UpdateCustomerCommand(customerUpdateContract);
            var customerUpdateStatusContract =
                await new UpdateCustomerCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .HandleAsync(updateCustomerCommand);
            
            return Ok(customerUpdateStatusContract);
        }
    }
}
