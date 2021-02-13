using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.Factories;
using Delivery.Domain.FrameWork.Context;
using Delivery.Order.Domain.Contracts.RestContracts.StripeOrder;
using Delivery.StripePayment.Domain.CommandHandlers.AccountCreation;
using Delivery.StripePayment.Domain.CommandHandlers.AccountCreation.Stripe.AccountLinkCreation;
using Delivery.StripePayment.Domain.CommandHandlers.AccountCreation.Stripe.LoginLinkCreation;
using Delivery.StripePayment.Domain.CommandHandlers.PaymentIntent.PaymentIntentConfirmation;
using Delivery.StripePayment.Domain.Contracts.Enums;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;
using Delivery.StripePayment.Domain.QueryHandlers.Stripe.AccountLinks;
using Delivery.StripePayment.Domain.QueryHandlers.Stripe.ApplicationFees;
using Delivery.StripePayment.Domain.QueryHandlers.Stripe.ConnectAccounts;
using Delivery.StripePayment.Domain.Services.ApplicationServices.StripeAccounts;
using Delivery.StripePayment.Domain.Services.ApplicationServices.StripeCapturePayment;
using Delivery.StripePayment.Domain.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

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

        [HttpGet("Account/GetConnectedAccounts")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetConnectedAccountsAsync()
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var connectAccountGetQuery = new ConnectAccountGetQuery(100, string.Empty, string.Empty);
            var accounts =
                await new ConnectAccountGetQueryHandler(serviceProvider, executingRequestContextAdapter).Handle(
                    connectAccountGetQuery);

            return Ok(accounts);
        }


        [HttpGet("ApplicationFees/GetAllApplicationFees")]
        [ProducesResponseType(typeof(StripeList<ApplicationFee>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAllApplicationFeesAsync()
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var applicationFeeGetQuery = new ApplicationFeeGetQuery(100, string.Empty, string.Empty);
            var applicationFees =
                await new ApplicationFeeGetQueryHandler(serviceProvider, executingRequestContextAdapter).Handle(
                    applicationFeeGetQuery);

            return Ok(applicationFees);
        }

        [HttpPost("Payment/CapturePayment")]
        [ProducesResponseType(typeof(StripePaymentCaptureCreationStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CapturePaymentAsync(
            StripePaymentCaptureCreationContract stripePaymentCaptureCreationContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult =
                await new StripePaymentCaptureCreationValidator().ValidateAsync(stripePaymentCaptureCreationContract);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }

            var stripeCapturePaymentServiceRequest =
                new StripeCapturePaymentServiceRequest(stripePaymentCaptureCreationContract);
            
            var stripeCapturePaymentServiceResult = await new StripeCapturePaymentService(serviceProvider, executingRequestContextAdapter)
                .ExecuteStripeCapturePaymentCreationWorkflowAsync(stripeCapturePaymentServiceRequest);
            
            return Ok(stripeCapturePaymentServiceResult.StripePaymentCaptureCreationStatusContract);
        }

        
        [HttpGet("Generate/GetGeneratedId")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        public  IActionResult GetGeneratedId()
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var id = executingRequestContextAdapter.GetShard().GenerateExternalId();

            return Ok(id);
        }
        
    }
}