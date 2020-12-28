using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class StripeWebhookController : Controller
    {
        // GET
        [HttpGet("Get/PaymentStatus")]
        public async Task<IActionResult> Index()
        {
            return Ok();
        }
    }
}