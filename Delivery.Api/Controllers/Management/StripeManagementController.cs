using System;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.FrameWork.Context;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;
using Delivery.StripePayment.Domain.Services.ApplicationServices.StripeAccounts;
using Delivery.StripePayment.Domain.Validators;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Management
{
    [Route("api/v1/management/stripe-management" , Name = "10 - Stripe management")]
    [PlatformSwaggerCategory(ApiCategory.Management)]
    [ApiController]
    public class StripeManagementController : Controller
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        ///  Stripe api endpoint
        /// </summary>
        /// <param name="serviceProvider"></param>
        public StripeManagementController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///  Create connect account
        /// </summary>
        /// <param name="stripeAccountCreationContract"></param>
        /// <returns></returns>
        [Route("Account/CreateAccount", Order = 1)]
        [HttpPost]
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

            var stripeAccountCreationServiceRequest =
                new StripeAccountCreationServiceRequest(stripeAccountCreationContract);

            var stripeAccountCreationServiceResult =
                await new StripeAccountCreationService(serviceProvider, executingRequestContextAdapter)
                    .ExecuteStripeAccountCreationWorkflowAsync(stripeAccountCreationServiceRequest);

            return Ok(stripeAccountCreationServiceResult.StripeAccountCreationStatusContract);
        }
    }
}