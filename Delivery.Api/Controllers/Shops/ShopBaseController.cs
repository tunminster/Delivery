using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Models;
using Delivery.Domain.Factories.Auth;
using Delivery.Domain.Models;
using Delivery.Shop.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopEmailVerification;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Delivery.Api.Controllers.Shops
{
    public class ShopBaseController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IJwtFactory jwtFactory;
        private IOptions<IdentityOptions> optionsAccessor;
        private readonly IPasswordHasher<Database.Models.ApplicationUser> passwordHasher;
        protected readonly IEnumerable<IUserValidator<Database.Models.ApplicationUser>> UserValidators;
        protected readonly IEnumerable<IPasswordValidator<Database.Models.ApplicationUser>> PasswordValidators;
        private readonly ILookupNormalizer keyNormalizer;
        private readonly IdentityErrorDescriber errors;
        private readonly ILogger<UserManager<Database.Models.ApplicationUser>> logger;
        
        public ShopBaseController(IServiceProvider serviceProvider, 
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<Database.Models.ApplicationUser> passwordHasher,
            IEnumerable<IUserValidator<Database.Models.ApplicationUser>> userValidators,
            IEnumerable<IPasswordValidator<Database.Models.ApplicationUser>> passwordValidators,
            IJwtFactory jwtFactory,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            ILogger<UserManager<Database.Models.ApplicationUser>> logger
        )
        {
            this.serviceProvider = serviceProvider;
            this.optionsAccessor = optionsAccessor;
            this.passwordHasher = passwordHasher;
            this.UserValidators = userValidators;
            this.PasswordValidators = passwordValidators;
            this.jwtFactory = jwtFactory;
            this.keyNormalizer = keyNormalizer;
            this.errors = errors;
            this.logger = logger;
        }
        
        protected async Task ConfirmEmailAsync(ShopEmailVerificationCheckContract shopEmailVerificationCheckContract,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            await using var applicationDbContext =
                await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

            var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                passwordHasher, UserValidators, PasswordValidators, keyNormalizer, errors, serviceProvider, logger);

            var user = await userManager.FindByEmailAsync(shopEmailVerificationCheckContract.Email);

            if (user == null)
            {
                throw new InvalidOperationException("Expected to be found user.").WithTelemetry(
                    executingRequestContextAdapter.GetTelemetryProperties());
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await userManager.ConfirmEmailAsync(user, token);
        }
        
        protected async Task<bool> CreateUserAsync(ShopCreationContract shopCreationContract, IExecutingRequestContextAdapter executingRequestContextAdapter, ValidationResult validationResult)
        {
            await using var applicationDbContext = await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

            using var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                passwordHasher, UserValidators, PasswordValidators, keyNormalizer, errors, serviceProvider,logger);
            
            var user = new Database.Models.ApplicationUser { UserName = shopCreationContract.EmailAddress, Email = shopCreationContract.EmailAddress };
            var result = await userManager.CreateAsync(user, shopCreationContract.Password);
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "ShopOwner");
                var claim = new Claim(ClaimData.ShopApiAccess.ClaimType, ClaimData.ShopApiAccess.ClaimValue, ClaimValueTypes.String);
                var groupClaim = new Claim("groups", executingRequestContextAdapter.GetShard().Key,
                    ClaimValueTypes.String);
                
                await userManager.AddClaimAsync(user, claim);
                await userManager.AddClaimAsync(user, groupClaim);
                
                return true;
            }

            foreach (var error in result.Errors)
            {
                validationResult.Errors.Add(new ValidationFailure(error.Code, error.Description));
            }
            
            return false;
        }
        
        protected async Task<ClaimsIdentity?> GetClaimsIdentityAsync(string userName, string password, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return await Task.FromResult<ClaimsIdentity?>(null);
            
            await using var applicationDbContext = await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

            var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                passwordHasher, UserValidators, PasswordValidators, keyNormalizer, errors, serviceProvider,logger);

            // get the user to verifty
            var userToVerify = await userManager.FindByNameAsync(userName);

            if (userToVerify == null) return await Task.FromResult<ClaimsIdentity>(null);
            
            var claimList = await userManager.GetClaimsAsync(userToVerify);
            
            var roleList = await userManager.GetRolesAsync(userToVerify);

            // check the credentials
            if (await userManager.CheckPasswordAsync(userToVerify, password))
            {
                return await Task.FromResult(jwtFactory.GenerateClaimsIdentity(userName, userToVerify.Id, claimList, roleList.ToList(),executingRequestContextAdapter));
            }

            // Credentials are invalid, or account doesn't exist
            return await Task.FromResult<ClaimsIdentity?>(null);
        }
    }
}