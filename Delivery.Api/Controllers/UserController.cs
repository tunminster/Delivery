using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Delivery.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using Delivery.Api.Helpers;
using Delivery.Api.Data;
using Delivery.Api.Entities;

namespace Delivery.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IEmailSender _emailSender;
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly ApplicationDbContext _appDbContext;

        public UserController(ILogger<UserController> logger,
             UserManager<ApplicationUser> userManager,
             SignInManager<ApplicationUser> signInManager,
             IEmailSender emailSender)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        // POST: api/User
        [HttpPost("register")]
        public async Task<IActionResult> Post([FromBody] RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _appDbContext.Customers.AddAsync(new Customer { IdentityId = user.Id, Username = user.Email });
                await _appDbContext.SaveChangesAsync();
                return new OkObjectResult("Account created");
            }
            return new BadRequestObjectResult(Errors.AddErrorsToModelState(result, ModelState));

        }

        
    }
}
