using System;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.FrameWork.Context;
using Delivery.Managements.Domain.Contracts.V1.RestContracts.Coupon;
using Delivery.Managements.Domain.Handlers.CommandHandlers.Coupon;
using Delivery.Managements.Domain.Validators.Coupon;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Management
{
    /// <summary>
    ///  Management coupon controller
    /// </summary>
    [Route("api/v1/management/coupon-management", Name = "13 - Management Coupon")]
    [PlatformSwaggerCategory(ApiCategory.Management)]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public class CouponManagementController : Controller
    {
        private readonly IServiceProvider serviceProvider;

        public CouponManagementController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        ///  Create a management coupon
        /// </summary>
        /// <remark>Create a category</remark>
        [Route("create-coupon", Order = 2)]
        [HttpPost]
        [ProducesResponseType(typeof(CouponManagementCreationStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddCouponAsync(CouponManagementCreationContract couponManagementCreationContract)
        {
            var validationResult = await new CouponManagementCreationValidator().ValidateAsync(couponManagementCreationContract);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            await new CreateCouponCodeCommandHandler(serviceProvider, executingRequestContextAdapter)
                .HandleAsync(new CreateCouponCodeCommand(couponManagementCreationContract));

            return Ok(new CouponManagementCreationStatusContract { CouponCode = couponManagementCreationContract.PromotionCode});
        }
    }
}