using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers
{
    public class StoreTypeController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}