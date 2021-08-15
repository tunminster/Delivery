using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers
{
    /// <summary>
    ///  Stripe webhooks
    /// </summary>
    [Route("api/v1/[controller]", Name = "8 - Stripe webhook")]
    [ApiController]
    [Authorize(Policy = "ApiUser")]
    [PlatformSwaggerCategory(ApiCategory.Customer)]
    public class StripeWebhookController : Controller
    {
        /// <summary>
        ///  Get payment status
        /// </summary>
        /// <returns></returns>
        [Route("Get/PaymentStatus", Order = 1)]
        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            return Ok();
        }
    }
}