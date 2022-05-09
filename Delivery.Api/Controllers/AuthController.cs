using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Delivery.Api.Helpers;
using Delivery.Api.Models;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Api.ViewModels;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Customer.Domain.Contracts.V1.RestContracts;
using Delivery.Customer.Domain.Handlers.CommandHandlers;
using Delivery.Customer.Domain.Validators;
using Delivery.Database.Context;
using Delivery.Domain.Factories;
using Delivery.Domain.Factories.Auth;
using Delivery.Domain.FrameWork.Context;
using Delivery.Domain.Models;
using Delivery.User.Domain.ApplicationServices;
using Delivery.User.Domain.Contracts.Apple;
using Delivery.User.Domain.Contracts.Facebook;
using Delivery.User.Domain.Contracts.Google;
using Delivery.User.Domain.Validators;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
    /// <summary>
    ///  Authentication controller
    /// </summary>
    [Route("api/[controller]", Name = "3 = Authentication")]
    [PlatformSwaggerCategory(ApiCategory.Customer)]
    public class AuthController : ControllerBase
    {
        private readonly IJwtFactory jwtFactory;
        private readonly JwtIssuerOptions jwtOptions;
        private readonly IServiceProvider serviceProvider;
        private readonly IPasswordHasher<Database.Models.ApplicationUser> passwordHasher;
        private readonly IEnumerable<IUserValidator<Database.Models.ApplicationUser>> userValidators;
        private readonly IEnumerable<IPasswordValidator<Database.Models.ApplicationUser>> passwordValidators;
        private readonly ILookupNormalizer keyNormalizer;
        private readonly IdentityErrorDescriber errors;
        private readonly ILogger<UserManager<Database.Models.ApplicationUser>> logger;
        private IOptions<IdentityOptions> optionsAccessor;

        public AuthController(
            IJwtFactory jwtFactory, 
            IOptions<JwtIssuerOptions> jwtOptions,
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
            this.jwtFactory = jwtFactory;
            this.jwtOptions = jwtOptions.Value;
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
        ///  Login
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("login", Order = 1)]
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody]CredentialsViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter(); 
            var identity = await GetClaimsIdentityAsync(model.UserName, model.Password, executingRequestContextAdapter);

            if (identity == null)
            {
                return BadRequest(Errors.AddErrorToModelState("login_failure", "Invalid username or password.", ModelState));
            }
            
            var jwt = await Tokens.GenerateJwtAsync(identity, jwtFactory, model.UserName, jwtOptions, new Newtonsoft.Json.JsonSerializerSettings { Formatting = Formatting.Indented });
            return new OkObjectResult(jwt);
        }
        
        /// <summary>
        ///  Foacebook login
        /// </summary>
        /// <param name="facebookLoginContract"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("account/login/facebook", Order = 2)]
        public async Task<IActionResult> FacebookLoginAsync([FromBody] FacebookLoginContract facebookLoginContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            await using var applicationDbContext = await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

            var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider,logger);
            
            var accountService = new AccountService(serviceProvider, userManager, jwtFactory, jwtOptions, executingRequestContextAdapter);
            
            applicationDbContext.SetExecutingRequestContextAdapter(serviceProvider,executingRequestContextAdapter);
            
            var authorizationTokens = await accountService.FacebookLoginTokenAsync(facebookLoginContract);
            
            return Ok(authorizationTokens);
        }

        /// <summary>
        ///  Google login
        /// </summary>
        /// <param name="googleLoginRequestContract"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("account/login/google", Order = 3)]
        public async Task<IActionResult> GoogleLoginAsync([FromBody] GoogleLoginRequestContract googleLoginRequestContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            await using var applicationDbContext = await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

            var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider,logger);
            
            var accountService = new AccountService(serviceProvider, userManager, jwtFactory, jwtOptions, executingRequestContextAdapter);
            
            applicationDbContext.SetExecutingRequestContextAdapter(serviceProvider,executingRequestContextAdapter);
            
            var authorizationTokens = await accountService.GoogleTokenLoginAsync(googleLoginRequestContract);
            
            return Ok(authorizationTokens);
        }

        /// <summary>
        ///  Apple login
        /// </summary>
        /// <param name="appleLoginRequestContract"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("account/login/apple", Order = 4)]
        public async Task<IActionResult> AppleLoginAsync([FromBody] AppleLoginRequestContract appleLoginRequestContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var validationResult =
                await new AppleLoginRequestValidator().ValidateAsync(appleLoginRequestContract);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            await using var applicationDbContext = await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

            var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider,logger);
            
            var accountService = new AccountService(serviceProvider, userManager, jwtFactory, jwtOptions, executingRequestContextAdapter);
            
            applicationDbContext.SetExecutingRequestContextAdapter(serviceProvider,executingRequestContextAdapter);
            
            var authorizationTokens = await accountService.AppleTokenLoginAsync(appleLoginRequestContract);
            
            return Ok(authorizationTokens);
        }
        
        /// <summary>
        ///  Request email verification
        /// </summary>
        /// <returns></returns>
        [Route("request-email-otp", Order = 5)]
        [ProducesResponseType(typeof(CustomerEmailVerificationStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_RequestEmailOtpAsync(
            [FromBody] CustomerEmailVerificationRequestContract customerEmailVerificationRequestContract)
        {
            var validationResult = await new CustomerEmailVerificationRequestValidator().ValidateAsync(customerEmailVerificationRequestContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var customerEmailVerificationStatusContract =
                await new CustomerEmailVerificationRequestCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .HandleAsync(new CustomerEmailVerificationRequestCommand(customerEmailVerificationRequestContract));

            return Ok(customerEmailVerificationStatusContract);
        }
        
        /// <summary>
        ///  Request email verification
        /// </summary>
        /// <returns></returns>
        [Route("verify-email-otp", Order = 6)]
        [ProducesResponseType(typeof(CustomerEmailVerificationStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_VerifyEmailOtpAsync(
            [FromBody] CustomerEmailVerificationContract customerEmailVerificationContract)
        {
            var validationResult = await new CustomerEmailVerificationValidator().ValidateAsync(customerEmailVerificationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var customerEmailVerificationStatusContract =
                await new CustomerEmailVerificationCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .HandleAsync(new CustomerEmailVerificationCommand(customerEmailVerificationContract));

            if (customerEmailVerificationStatusContract.Status == "approved")
            {
                //await ConfirmEmailAsync(customerEmailVerificationContract, executingRequestContextAdapter);
            }

            return Ok(customerEmailVerificationStatusContract);
        }
        
        /// <summary>
        ///  Request reset password otp
        /// </summary>
        /// <returns></returns>
        [Route("request-reset-password-otp", Order = 7)]
        [ProducesResponseType(typeof(CustomerResetPasswordStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_RequestResetPasswordAsync(
            [FromBody] CustomerResetPasswordRequestContract customerResetPasswordRequestContract)
        {
            var validationResult = await new CustomerResetPasswordRequestValidator().ValidateAsync(customerResetPasswordRequestContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var customerResetPasswordStatusContract =
                await new CustomerResetPasswordRequestCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .HandleAsync(new CustomerResetPasswordRequestCommand(customerResetPasswordRequestContract));

            return Ok(customerResetPasswordStatusContract);
        }
        
        /// <summary>
        ///  Verify otp and reset password
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [Route("verify-reset-password", Order = 8)]
        [ProducesResponseType(typeof(CustomerResetPasswordStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_VerifyResetPasswordOtpAsync([FromBody] CustomerResetPasswordCreationContract customerResetPasswordCreationContract)
        {
            var validationResult =
                await new CustomerResetPasswordVerificationValidator().ValidateAsync(
                    customerResetPasswordCreationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var customerResetPasswordStatusContract =
                await new CustomerResetPasswordVerificationCommandHandler(serviceProvider,
                        executingRequestContextAdapter)
                    .HandleAsync(new CustomerResetPasswordVerificationCommand(customerResetPasswordCreationContract));

            if (customerResetPasswordStatusContract.Status == "approved")
            {
                await using var applicationDbContext =
                    await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

                var customer = applicationDbContext.Customers.FirstOrDefault(x =>
                    x.Username.ToLower() == customerResetPasswordCreationContract.Email.ToLower());

                if (customer == null)
                {
                    return Ok(new CustomerResetPasswordStatusContract { Status = "error", Valid = false });
                }
                
                var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

                var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                    passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider, logger);

                var user = await userManager.FindByEmailAsync(customerResetPasswordCreationContract.Email);

                if (user == null)
                {
                    throw new InvalidOperationException($"Expected to be found {customerResetPasswordCreationContract.Email} user.").WithTelemetry(
                        executingRequestContextAdapter.GetTelemetryProperties());
                }
                
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResetResult = await userManager.ResetPasswordAsync(user, token, customerResetPasswordCreationContract.Password);

                if (passwordResetResult.Succeeded)
                {
                    return Ok(customerResetPasswordStatusContract);
                }

                foreach (var error in passwordResetResult.Errors)
                {
                    validationResult.Errors.Add(new ValidationFailure(error.Code, error.Description));
                }
                
                return validationResult.ConvertToBadRequest();
            }
            
            var errorResult = customerResetPasswordStatusContract with
            {
                Status = "error",
                Valid = false
            };

            return Ok(errorResult);
        }
        
        private async Task ConfirmEmailAsync(CustomerEmailVerificationContract customerEmailVerificationContract,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            await using var applicationDbContext =
                await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

            var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider, logger);

            var user = await userManager.FindByEmailAsync(customerEmailVerificationContract.Email);

            if (user == null)
            {
                throw new InvalidOperationException("Expected to be found user.").WithTelemetry(
                    executingRequestContextAdapter.GetTelemetryProperties());
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await userManager.ConfirmEmailAsync(user, token);
        }

        private async Task<ClaimsIdentity> GetClaimsIdentityAsync(string userName, string password, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return await Task.FromResult<ClaimsIdentity>(null);
            
            await using var applicationDbContext = await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

            var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider,logger);

            // get the user to verifty
            var userToVerify = await userManager.FindByNameAsync(userName);

            if (userToVerify == null) return await Task.FromResult<ClaimsIdentity>(null);
            
            var claimList = await userManager.GetClaimsAsync(userToVerify);
            var roleList = await userManager.GetRolesAsync(userToVerify);

            // check the credentials
            if (await userManager.CheckPasswordAsync(userToVerify, password))
            {
                return await Task.FromResult(jwtFactory.GenerateClaimsIdentity(userName, userToVerify.Id, claimList, roleList.ToList(), executingRequestContextAdapter));
            }

            // Credentials are invalid, or account doesn't exist
            return await Task.FromResult<ClaimsIdentity>(null);
        }
    }
}
