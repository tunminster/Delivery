﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Delivery.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Delivery.Api.Helpers;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Microsoft.AspNetCore.Authorization;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Customer.Domain.Contracts.V1.RestContracts;
using Delivery.Customer.Domain.Handlers.CommandHandlers;
using Delivery.Database.Context;
using Delivery.Database.Models;
using Delivery.Domain.FrameWork.Context;
using Delivery.User.Domain.Contracts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using ApplicationUser = Delivery.Api.Models.ApplicationUser;

namespace Delivery.Api.Controllers
{
    
    /// <summary>
    ///  User apis
    /// </summary>
    [Route("api/[controller]", Name="9 - User apis")]
    [ApiController]
    [PlatformSwaggerCategory(ApiCategory.Customer)]
    public class UserController : ControllerBase
    {
        private readonly IPasswordHasher<Database.Models.ApplicationUser> passwordHasher;
        private readonly IEnumerable<IUserValidator<Database.Models.ApplicationUser>> userValidators;
        private readonly IEnumerable<IPasswordValidator<Database.Models.ApplicationUser>> passwordValidators;
        private readonly ILookupNormalizer keyNormalizer;
        private readonly IdentityErrorDescriber errors;
        private readonly ILogger<UserManager<Database.Models.ApplicationUser>> logger;
        private IOptions<IdentityOptions> optionsAccessor;
        private readonly IServiceProvider serviceProvider;

        public UserController(
             IServiceProvider serviceProvider,
             IOptions<IdentityOptions> optionsAccessor,
             IPasswordHasher<Database.Models.ApplicationUser> passwordHasher,
             IEnumerable<IUserValidator<Database.Models.ApplicationUser>> userValidators,
             IEnumerable<IPasswordValidator<Database.Models.ApplicationUser>> passwordValidators,
             ILookupNormalizer keyNormalizer,
             IdentityErrorDescriber errors,
             ILogger<UserManager<Database.Models.ApplicationUser>> logger
             )
        {
            this.serviceProvider = serviceProvider;
            this.optionsAccessor = optionsAccessor;
            this.passwordHasher = passwordHasher;
            this.userValidators = userValidators;
            this.passwordValidators = passwordValidators;
            this.keyNormalizer = keyNormalizer;
            this.errors = errors;
            this.logger = logger;
        }

        /// <summary>
        ///  Register user endpoint 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("register", Order = 1)]
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            await using var applicationDbContext = await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

            using var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider,logger);
            
            var user = new Database.Models.ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Customer");
                var claim = new Claim(ClaimData.CustomerApiAccess.ClaimType, ClaimData.CustomerApiAccess.ClaimValue, ClaimValueTypes.String);
                var groupClaim = new Claim("groups", executingRequestContextAdapter.GetShard().Key,
                    ClaimValueTypes.String);
                await userManager.AddClaimAsync(user, claim);


                await userManager.AddClaimAsync(user, groupClaim);
                var customerCreationContract = new CustomerCreationContract
                {
                    IdentityId = user.Id, 
                    Username = user.Email,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    ContactNumber = string.Empty
                };

                var createCustomerCommand = new CreateCustomerCommand(customerCreationContract);
                var createCustomerCommandHandler =
                    new CreateCustomerCommandHandler(serviceProvider, executingRequestContextAdapter); 
                await createCustomerCommandHandler.HandleAsync(createCustomerCommand);
                
                return new OkObjectResult("Account created");
            }
            return new BadRequestObjectResult(Errors.AddErrorsToModelState(result, ModelState));
        }

        /// <summary>
        ///  Get user
        /// </summary>
        /// <returns></returns>
        [Route("GetUser", Order = 2)]
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(UserContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetUserAsync()
        {
            var userName = HttpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var result = new UserContract { UserName = userName! };
            return Ok(await Task.FromResult(result));
        }
        
        /// <summary>
        ///  Delete user
        /// </summary>
        /// <returns></returns>
        [Route("delete-account", Order = 3)]
        [HttpPost]
        [ProducesResponseType(typeof(UserContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeleteAccountAsync()
        {
            var userName = HttpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            await new DeleteCustomerCommandHandler(serviceProvider, executingRequestContextAdapter).HandleAsync(
                new DeleteCustomerCommand(userName!));

            return Ok();
        }
        
    }
}
