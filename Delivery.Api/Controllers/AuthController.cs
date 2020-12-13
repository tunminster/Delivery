using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Delivery.Api.Auth;
using Delivery.Api.Helpers;
using Delivery.Api.Models;
using Delivery.Api.ViewModels;
using Delivery.Database.Context;
using Delivery.Domain.FrameWork.Context;
using Delivery.User.Domain.ApplicationServices;
using Delivery.User.Domain.CommandHandlers;
using Delivery.User.Domain.Contracts.Facebook;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Delivery.Api.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly SignInManager<Database.Models.ApplicationUser> signInManager;
        private readonly UserManager<Database.Models.ApplicationUser> userManager;
        private readonly ApplicationDbContext applicationDbContext;

        private readonly IJwtFactory jwtFactory;
        private readonly JwtIssuerOptions jwtOptions;
        private readonly IServiceProvider serviceProvider;

        public AuthController(
            UserManager<Database.Models.ApplicationUser> userManager,
            SignInManager<Database.Models.ApplicationUser> signInManager,
            IJwtFactory jwtFactory, 
            IOptions<JwtIssuerOptions> jwtOptions,
            IServiceProvider serviceProvider,
            ApplicationDbContext applicationDbContext
            )
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.jwtFactory = jwtFactory;
            this.jwtOptions = jwtOptions.Value;
            this.serviceProvider = serviceProvider;

            this.applicationDbContext = applicationDbContext;
        }

        [HttpPost("login")]
        public async Task<IActionResult> PostAsync([FromBody]CredentialsViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var identity = await GetClaimsIdentityAsync(model.UserName, model.Password);

            if (identity == null)
            {
                return BadRequest(Errors.AddErrorToModelState("login_failure", "Invalid username or password.", ModelState));
            }
            
            var jwt = await Tokens.GenerateJwt(identity, jwtFactory, model.UserName, jwtOptions, new Newtonsoft.Json.JsonSerializerSettings { Formatting = Formatting.Indented });
            return new OkObjectResult(jwt);
        }
        
        [HttpPost]
        [Route("account/login/facebook")]
        public async Task<IActionResult> FacebookLoginAsync([FromBody] FacebookLoginContract facebookLoginContract)
        {
            var accountService = new AccountService(serviceProvider, userManager);
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            applicationDbContext.SetExecutingRequestContextAdapter(serviceProvider,executingRequestContextAdapter);
            
            var authorizationTokens = await accountService.FacebookLoginAsync(facebookLoginContract);
            
            return Ok(authorizationTokens);
        }

        private async Task<ClaimsIdentity> GetClaimsIdentityAsync(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return await Task.FromResult<ClaimsIdentity>(null);

            // get the user to verifty
            var userToVerify = await userManager.FindByNameAsync(userName);

            if (userToVerify == null) return await Task.FromResult<ClaimsIdentity>(null);

            // check the credentials
            if (await userManager.CheckPasswordAsync(userToVerify, password))
            {
                return await Task.FromResult(jwtFactory.GenerateClaimsIdentity(userName, userToVerify.Id));
            }

            // Credentials are invalid, or account doesn't exist
            return await Task.FromResult<ClaimsIdentity>(null);
        }
    }
}
