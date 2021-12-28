using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.Factories;
using Delivery.Domain.FrameWork.Context;
using Delivery.StripePayment.Domain.CommandHandlers.AccountCreation;
using Delivery.StripePayment.Domain.CommandHandlers.AccountCreation.Stripe.AccountLinkCreation;
using Delivery.StripePayment.Domain.CommandHandlers.AccountCreation.Stripe.LoginLinkCreation;
using Delivery.StripePayment.Domain.Contracts.Enums;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.CouponPayments;
using Delivery.StripePayment.Domain.Handlers.QueryHandlers.CouponPayments;
using Delivery.StripePayment.Domain.Handlers.QueryHandlers.Stripe.ApplicationFees;
using Delivery.StripePayment.Domain.Handlers.QueryHandlers.Stripe.ConnectAccounts;
using Delivery.StripePayment.Domain.Services.ApplicationServices.StripeAccounts;
using Delivery.StripePayment.Domain.Services.ApplicationServices.StripeCapturePayment;
using Delivery.StripePayment.Domain.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Delivery.Api.Controllers
{
    /// <summary>
    ///  Stripe api endpoint allows to make payment
    /// </summary>
    [Route("api/v1/[controller]", Name="8 - Stripe apis")]
    [ApiController]
    [Authorize(Policy = "CustomerApiUser")]
    [PlatformSwaggerCategory(ApiCategory.Customer)]
    public class StripeController : Controller
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        ///  Stripe api endpoint
        /// </summary>
        /// <param name="serviceProvider"></param>
        public StripeController(IServiceProvider serviceProvider)
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

        /// <summary>
        ///  Create login link
        /// </summary>
        /// <param name="stripeLoginLinkCreationContract"></param>
        /// <returns></returns>
        [Route("Account/CreatLoginLink", Order = 2)]
        [HttpPost]
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
        
        /// <summary>
        ///  Create on boarding link 
        /// </summary>
        /// <param name="stripeAccountLinkCreationContract"></param>
        /// <returns></returns>
        [Route("Account/CreatOnBoardingLink", Order = 3)]
        [HttpPost]
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
                await new AccountLinkCreationCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(accountLinkCreationCommand);

            return Ok(stripeAccountLinkCreationStatusContract);

        }

        /// <summary>
        ///  Get connected account
        /// </summary>
        /// <returns></returns>
        [Route("Account/GetConnectedAccounts", Order = 4)]
        [HttpGet]
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


        /// <summary>
        ///  Get all application fees
        /// </summary>
        /// <returns></returns>
        [Route("ApplicationFees/GetAllApplicationFees", Order = 5)]
        [HttpGet]
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

        /// <summary>
        ///  Create capture payment
        /// </summary>
        /// <param name="stripePaymentCaptureCreationContract"></param>
        /// <returns></returns>
        [Route("Payment/CapturePayment", Order = 6)]
        [HttpPost]
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

        /// <summary>
        ///  Get generated id
        /// </summary>
        /// <returns></returns>
        [Route("Generate/GetGeneratedId", Order = 7)]
        [HttpGet]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        public  IActionResult GetGeneratedId()
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var id = executingRequestContextAdapter.GetShard().GenerateExternalId();

            return Ok(id);
        }

        /// <summary>
        ///  Confirm coupon code
        /// </summary>
        /// <remarks>
        ///  This endpoint allows to verify the coupon code
        /// </remarks>
        [Route("confirm-coupon-code", Order = 8)]
        [HttpPost]
        [ProducesResponseType(typeof(CouponCodeStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Verify_PromoCodeAsync(CouponCodeConfirmationQueryContract couponCodeConfirmationQueryContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var couponCodeConfirmationQuery = new CouponCodeConfirmationQuery(couponCodeConfirmationQueryContract);

            var couponCodeConfirmationQueryStatusContract =
                await new CouponCodeConfirmationQueryHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(couponCodeConfirmationQuery);

            return Ok(couponCodeConfirmationQueryStatusContract);
        }
        
    }
}