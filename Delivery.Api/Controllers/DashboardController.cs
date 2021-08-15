
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Delivery.Api.Models;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Database.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Api.Controllers
{
    /// <summary>
    ///  Dashboard apis
    /// </summary>
    [Authorize(Policy = "ApiUser")]
    [Route("api/[controller]", Name = "5 - Dashboard")]
    [ApiController]
    [PlatformSwaggerCategory(ApiCategory.Customer)]
    public class DashboardController : ControllerBase
    {
        private readonly ClaimsPrincipal _caller;
        private readonly ApplicationDbContext _appDbContext;

        public DashboardController(UserManager<ApplicationUser> userManager, ApplicationDbContext appDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _caller = httpContextAccessor.HttpContext.User;
            _appDbContext = appDbContext;
        }

        /// <summary>
        ///  Home
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [Route("home", Order = 1)]
        [HttpGet]
        public async Task<IActionResult> Home()
        {
            // retrieve the user info
            // ToDo: remove dbcontext calling here
            // var userId = _caller.Claims.Single(c => c.Type == "id");
            // var customer = await _appDbContext.Customers.Include(c => c.Identity).SingleAsync(c => c.Identity.Id == userId.Value);
            //
            // return new OkObjectResult(new
            // {
            //     Message = "This is secure API and user data!",
            //     customer.Identity.Email,
            //     customer.Identity.UserName
            // });
            
             throw new NotImplementedException();
        }
    }
}