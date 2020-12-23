using System;
using System.Net;
using System.Threading.Tasks;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.FrameWork.Context;
using Delivery.Order.Domain.Contracts.RestContracts.StripeOrder;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;
using Delivery.StripePayment.Domain.Validators;
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

        [HttpPost("Account/CreateAccount")]
        [ProducesResponseType(typeof(StripeAccountCreationStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateConnectAccountAsync(StripeAccountCreationContract stripeAccountCreationContract)
        {
            var validationResult = await new StripeAccountCreationValidator().ValidateAsync(stripeAccountCreationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }

            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            
            throw new NotImplementedException();
        }
        
    }
}