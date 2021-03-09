using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Customer.Domain.CommandHandlers;
using Delivery.Customer.Domain.Contracts.RestContracts;
using Delivery.Database.Models;
using Delivery.Domain.Factories;
using Delivery.Domain.Factories.Auth;
using Delivery.Domain.Models;
using Delivery.User.Domain.CommandHandlers;
using Delivery.User.Domain.Contracts.Apple;
using Delivery.User.Domain.Contracts.Facebook;
using Delivery.User.Domain.Contracts.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Delivery.User.Domain.ApplicationServices
{
    public class AccountService
    {
        private readonly JwtCommandHandler jwtCommandHandler;
        private readonly IServiceProvider serviceProvider;
        private readonly UserManager<Database.Models.ApplicationUser> userManager;
        private readonly IJwtFactory jwtFactory;
        private readonly JwtIssuerOptions jwtOptions;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public AccountService(IServiceProvider serviceProvider, UserManager<Database.Models.ApplicationUser> userManager, 
            IJwtFactory jwtFactory, 
            JwtIssuerOptions jwtOptions,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            jwtCommandHandler = new JwtCommandHandler(serviceProvider);
            this.serviceProvider = serviceProvider;
            this.userManager = userManager;
            this.jwtFactory = jwtFactory;
            this.jwtOptions = jwtOptions;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        
        public async Task<FacebookAuthorizationTokenContract> FacebookLoginAsync(FacebookLoginContract facebookLoginContract)
        {
            if (string.IsNullOrEmpty(facebookLoginContract.FacebookToken))
            {
                throw new Exception("Token is null or empty");
            }
            
            var facebookUser = await new FacebookService(executingRequestContextAdapter).GetUserFromFacebookAsync(facebookLoginContract.FacebookToken);
            var facebookEmail = $"{facebookUser.Email}_{UniqueIdFactory.UniqueFacebookId()}@facebooklogin.com";
            var user = new Database.Models.ApplicationUser { UserName = facebookUser.Email, Email = facebookEmail };
            var domainUser = await userManager.FindByNameAsync(facebookUser.Email);
            if (domainUser == null)
            {
                
                user = new Database.Models.ApplicationUser { UserName = $"{facebookUser.FirstName}_{facebookUser.LastName}", Email = facebookEmail };
                var result = await userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    var userLoginInfo = new UserLoginInfo
                        ("Facebook", "Facebook", user.UserName);
                        
                    result = await userManager.AddLoginAsync(user, userLoginInfo);
                    if (result.Succeeded)
                    {
                        result = await userManager.AddToRoleAsync(user, "Customer");
                        var claim = new Claim(ClaimData.JwtClaimIdentifyClaim.ClaimType, ClaimData.JwtClaimIdentifyClaim.ClaimValue, ClaimValueTypes.String);
                        await userManager.AddClaimAsync(user, claim);
                    }
                }
            }
            
            return await CreateAccessTokensAsync(user, "Customer");
        }

        public async Task<string> AppleTokenLoginAsync(AppleLoginRequestContract appleLoginRequestContract)
        {
            var user = new Database.Models.ApplicationUser
                {UserName = appleLoginRequestContract.Email, Email = appleLoginRequestContract.Email};

            var domainUser = await userManager.FindByEmailAsync(appleLoginRequestContract.Email);

            if (domainUser == null)
            {
                var createUserResult = await userManager.CreateAsync(user);

                switch (createUserResult.Succeeded)
                {
                    case false:
                        throw new InvalidOperationException($"Apple login creating user has failed with {appleLoginRequestContract.Email}")
                            .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());
                    case true:
                    {
                        var userLoginInfo = new UserLoginInfo
                            ("Apple", user.UserName, user.UserName);
                        
                        var createUserLoginResult = await userManager.AddLoginAsync(user, userLoginInfo);

                        switch (createUserLoginResult.Succeeded)
                        {
                            case false:
                                throw new InvalidOperationException($"Apple creating user login info has failed with {appleLoginRequestContract.Email}")
                                    .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());
                            case true:
                            {
                                await userManager.AddToRoleAsync(user, "Customer");
                                var claim = new Claim(ClaimData.JwtClaimIdentifyClaim.ClaimType, ClaimData.JwtClaimIdentifyClaim.ClaimValue, ClaimValueTypes.String);
                        
                                var groupClaim = new Claim("groups", executingRequestContextAdapter.GetShard().Key,
                                    ClaimValueTypes.String);
                                await userManager.AddClaimAsync(user, claim);
                                await userManager.AddClaimAsync(user, groupClaim);
                        
                                var customerCreationContract = new CustomerCreationContract
                                {
                                    IdentityId = user.Id, 
                                    Username = user.Email,
                                    FirstName = appleLoginRequestContract.GivenName,
                                    LastName = appleLoginRequestContract.FamilyName,
                                    ContactNumber = string.Empty
                                };

                                var createCustomerCommand = new CreateCustomerCommand(customerCreationContract);
                                var createCustomerCommandHandler =
                                    new CreateCustomerCommandHandler(serviceProvider, executingRequestContextAdapter); 
                                await createCustomerCommandHandler.Handle(createCustomerCommand);
                                break;
                            }
                        }

                        break;
                    }
                }
            }
            
            return await CreateAccessTokenStringAsync(user, "Customer");
        }

        public async Task<string> GoogleTokenLoginAsync(GoogleLoginRequestContract googleLoginRequestContract)
        {
            if (string.IsNullOrEmpty(googleLoginRequestContract.IdToken))
            {
                throw new InvalidOperationException("Token must be provided");
            }

            var googleUser = await new GoogleService(executingRequestContextAdapter).GetUserFromGoogleAsync(googleLoginRequestContract.IdToken);
            var user = new Database.Models.ApplicationUser {UserName = googleUser.Email, Email = googleUser.Email};
            
            var domainUser = await userManager.FindByNameAsync(googleUser.Email);
            
            if (domainUser == null)
            {
                user = new Database.Models.ApplicationUser { UserName = googleUser.Email, Email = googleUser.Email };
                var createUserResult = await userManager.CreateAsync(user);

                switch (createUserResult.Succeeded)
                {
                    case false:
                        throw new InvalidOperationException($"Google creating user has failed with {googleUser.Email}")
                            .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());
                    case true:
                    {
                        var userLoginInfo = new UserLoginInfo
                            ("Google", user.UserName, user.UserName);
                        
                        var createUserLoginResult = await userManager.AddLoginAsync(user, userLoginInfo);

                        switch (createUserLoginResult.Succeeded)
                        {
                            case false:
                                throw new InvalidOperationException($"Google creating user login info has failed with {googleUser.Email}")
                                    .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());
                            case true:
                            {
                                await userManager.AddToRoleAsync(user, "Customer");
                                var claim = new Claim(ClaimData.JwtClaimIdentifyClaim.ClaimType, ClaimData.JwtClaimIdentifyClaim.ClaimValue, ClaimValueTypes.String);
                        
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
                                await createCustomerCommandHandler.Handle(createCustomerCommand);
                                break;
                            }
                        }

                        break;
                    }
                }
            }
            return await CreateAccessTokenStringAsync(user, "Customer");

        }
        
        public async Task<string> FacebookLoginTokenAsync(FacebookLoginContract facebookLoginContract)
        {
            if (string.IsNullOrEmpty(facebookLoginContract.FacebookToken))
            {
                throw new Exception("Token is null or empty");
            }

            var facebookUser = await new FacebookService(executingRequestContextAdapter).GetUserFromFacebookAsync(facebookLoginContract.FacebookToken);
            var user = new Database.Models.ApplicationUser { UserName = facebookUser.Email, Email = facebookUser.Email };
            var domainUser = await userManager.FindByNameAsync(facebookUser.Email);
            if (domainUser == null)
            {
                
                user = new Database.Models.ApplicationUser { UserName = facebookUser.Email, Email = facebookUser.Email };
                var createUserResult = await userManager.CreateAsync(user);

                switch (createUserResult.Succeeded)
                {
                    case false:
                        throw new InvalidOperationException($"Facebook creating user has failed with {facebookUser.Email}")
                            .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());
                    case true:
                    {
                        var userLoginInfo = new UserLoginInfo
                            ("Facebook", user.UserName, user.UserName);
                        
                        var createUserLoginResult = await userManager.AddLoginAsync(user, userLoginInfo);

                        switch (createUserLoginResult.Succeeded)
                        {
                            case false:
                                throw new InvalidOperationException(
                                        $"Facebook creating user login info has failed with {facebookUser.Email}")
                                    .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());
                            case true:
                            {
                                await userManager.AddToRoleAsync(user, "Customer");
                                var claim = new Claim(ClaimData.JwtClaimIdentifyClaim.ClaimType, ClaimData.JwtClaimIdentifyClaim.ClaimValue, ClaimValueTypes.String);
                        
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
                                await createCustomerCommandHandler.Handle(createCustomerCommand);
                                break;
                            }
                        }

                        break;
                    }
                }
            }
            
            return await CreateAccessTokenStringAsync(user, "Customer");
        }

        private async Task<FacebookAuthorizationTokenContract> CreateAccessTokensAsync(ApplicationUser user, string roleName)
        {
            var accessToken = jwtCommandHandler.CreateAccessToken(Guid.Parse(user.Id), user.Email, roleName);
            var refreshToken = jwtCommandHandler.CreateRefreshToken(Guid.Parse(user.Id));
            
            return new FacebookAuthorizationTokenContract { AccessToken = accessToken, RefreshToken = refreshToken };
        }

        public async Task<string> CreateAccessTokenStringAsync(ApplicationUser user, string roleName)
        {
            var identity = await GetClaimsIdentityAsync(user.UserName);
            
            var jwtToken = await Tokens.GenerateJwtAsync(identity, jwtFactory, user.UserName, jwtOptions, new Newtonsoft.Json.JsonSerializerSettings { Formatting = Formatting.Indented });
            return jwtToken;
        }
        
        private async Task<ClaimsIdentity> GetClaimsIdentityAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return await Task.FromResult<ClaimsIdentity>(null);
            }
                

            // get the user to verifty
            var userToVerify = await userManager.FindByNameAsync(userName);

            if (userToVerify == null) return await Task.FromResult<ClaimsIdentity>(null);

            var claimList = await userManager.GetClaimsAsync(userToVerify);

            // check the credentials
            return await Task.FromResult(jwtFactory.GenerateClaimsIdentity(userName, userToVerify.Id, claimList, executingRequestContextAdapter));

        }
    }
}