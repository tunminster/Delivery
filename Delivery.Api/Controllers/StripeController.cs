using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.FrameWork.Context;
using Delivery.Order.Domain.Contracts.RestContracts.StripeOrder;
using Delivery.StripePayment.Domain.CommandHandlers.AccountCreation;
using Delivery.StripePayment.Domain.CommandHandlers.AccountCreation.Stripe.AccountLinkCreation;
using Delivery.StripePayment.Domain.CommandHandlers.AccountCreation.Stripe.LoginLinkCreation;
using Delivery.StripePayment.Domain.Contracts.Enums;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;
using Delivery.StripePayment.Domain.QueryHandlers.Stripe.AccountLinks;
using Delivery.StripePayment.Domain.Services.ApplicationServices.StripeAccounts;
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

            var stripeAccountCreationServiceRequest =
                new StripeAccountCreationServiceRequest(stripeAccountCreationContract);

            var stripeAccountCreationServiceResult =
                await new StripeAccountCreationService(serviceProvider, executingRequestContextAdapter)
                    .ExecuteStripeAccountCreationWorkflowAsync(stripeAccountCreationServiceRequest);

            return Ok(stripeAccountCreationServiceResult.StripeAccountCreationStatusContract);
        }

        [HttpPost("Account/CreatLoginLink")]
        [ProducesResponseType(typeof(StripeLoginLinkCreationStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateConnectAccountLoginLinkAsync(
            StripeLoginLinkCreationContract stripeLoginLinkCreationContract)
        {
            var validationResult =
                await new StripeLoginLinkCreationValidator().ValidateAsync(stripeLoginLinkCreationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var loginLinkCreationCommand = new LoginLinkCreationCommand(stripeLoginLinkCreationContract);
            var loginLinkCreationStatusContract =
                new LoginLinkCreationCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(
                    loginLinkCreationCommand);

            return Ok(loginLinkCreationStatusContract);
        }
        
        [HttpPost("Account/CreatOnBoardingLink")]
        [ProducesResponseType(typeof(StripeAccountLinkCreationStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateAccountOnBoardingLinkAsync(
            StripeAccountLinkCreationContract stripeAccountLinkCreationContract)
        {
            var validationResult =
                await new StripeAccountLinkCreationValidator().ValidateAsync(stripeAccountLinkCreationContract);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var accountLinkCreationCommand =
                new AccountLinkCreationCommand(stripeAccountLinkCreationContract);

            var stripeAccountLinkCreationStatusContract =
                await new AccountLinkCreationCommandHandler(serviceProvider).Handle(accountLinkCreationCommand);

            return Ok(stripeAccountLinkCreationStatusContract);

        }
        
        
    }
}