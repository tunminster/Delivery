using System;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.FrameWork.Context;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.CouponPayments;
using Delivery.StripePayment.Domain.Handlers.CommandHandlers.CouponPayments;
using Delivery.StripePayment.Domain.Handlers.QueryHandlers.CouponPayments;
using Delivery.StripePayment.Domain.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers
{
    /// <summary>
    ///  Coupon controller
    /// </summary>
    [Route("api/v1/coupon", Name = "16 - Coupon")]
    [PlatformSwaggerCategory(ApiCategory.Customer)]
    [ApiController]
    [Authorize]
    public class CouponController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        public CouponController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///  Confirm coupon
        /// </summary>
        /// <returns></returns>
        [Route("confirm-coupon", Order = 1)]
        [HttpPost]
        [ProducesResponseType(typeof(CouponCodeStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Confirm_CouponCodeAsync(CouponCodeConfirmationQueryContract couponCodeConfirmationQueryContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult =
                await new CouponCodeConfirmationValidator().ValidateAsync(couponCodeConfirmationQueryContract);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var couponCodeStatusContract = await new CouponCodeConfirmationQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(new CouponCodeConfirmationQuery(couponCodeConfirmationQueryContract));
            
            return Ok(couponCodeStatusContract);
        }

        /// <summary>
        ///  Get coupon
        /// </summary>
        /// <returns></returns>
        [Route("get-coupon", Order = 2)]
        [HttpGet]
        [ProducesResponseType(typeof(CouponCodeContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get_CouponCodeAsync(string couponCode)
        {
            if (string.IsNullOrEmpty(couponCode))
            {
                return $"Invalid {nameof(couponCode)}".ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var couponCodeContract = await new CouponCodeGetQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(new CouponCodeGetQuery(couponCode));

            return Ok(couponCodeContract);
        }
        
    }
}