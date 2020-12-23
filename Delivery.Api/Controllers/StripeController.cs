using System;
using System.Net;
using System.Threading.Tasks;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Order.Domain.Contracts.RestContracts.StripeOrder;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiUser")]
    public class StripeController : Controller
    {
        private readonly IServiceProvider serviceProvider;

        public StripeController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [HttpPost("Payment/CreatePaymentIntent")]
        [ProducesResponseType(typeof(StripeAccountCreationStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateConnectAccountAsync(StripeAccountCreationContract stripeAccountCreationContract)
        {
            throw new NotImplementedException();
        }
        
    }
}