using System;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Domain.FrameWork.Context;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.CouponPayments;
using Delivery.StripePayment.Domain.Handlers.CommandHandlers.CouponPayments;
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
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(CouponCodeStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Confirm_CouponCodeAsync(string couponCode)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var couponCodeConfirmationCommand = new CouponCodeConfirmationCommand
            (new CouponCodeConfirmationQueryContract
            {
                CouponCode = couponCode
            });

            var couponCodeConfirmationQueryStatusContract =
                await new CouponCodeConfirmationCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(couponCodeConfirmationCommand);
            
            return Ok(couponCodeConfirmationQueryStatusContract);
        }
    }
}