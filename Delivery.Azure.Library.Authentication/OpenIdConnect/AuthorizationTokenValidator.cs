using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.Authentication.OpenIdConnect.Extensions;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Threading;

namespace Delivery.Azure.Library.Authentication.OpenIdConnect
{
    public static class AuthorizationTokenValidator
	{
		/// <summary>
		///     Validates an id_token which was provided at the end of the OAuth/OIDC authorization login flow
		/// </summary>
		/// <param name="serviceProvider">The application kernel</param>
		/// <param name="token">The id token extracted from the Authorization header</param>
		/// <param name="nonce">An optional nonce to validate. Use this only for the implicit flow.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <exception cref="SecurityTokenValidationException">The id token could not be validated</exception>
		public static async Task<JwtSecurityToken> ValidateIdTokenAsync(IServiceProvider serviceProvider, string token, string? nonce = null, CancellationToken cancellationToken = default)
		{
			var validationParameters = await GetTokenValidationParametersAsync(serviceProvider, cancellationToken);

			var principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out var rawValidatedToken);
			if (principal == null)
			{
				throw new SecurityTokenValidationException("No principal was returned from token validation");
			}

			var jwtSecurityToken = (JwtSecurityToken) rawValidatedToken;
			var expectedAlg = SecurityAlgorithms.RsaSha256; //Okta uses RS256

			if (jwtSecurityToken.Header?.Alg == null || jwtSecurityToken.Header?.Alg != expectedAlg)
			{
				throw new SecurityTokenValidationException($"The algorithm must be RS256, found: {jwtSecurityToken.Header?.Alg}");
			}

			if (string.IsNullOrEmpty(nonce))
			{
				return jwtSecurityToken;
			}

			var nonceMatches = jwtSecurityToken.Payload.TryGetValue("nonce", out var rawNonce) && rawNonce.ToString() == nonce;

			if (!nonceMatches)
			{
				throw new SecurityTokenValidationException($"The nonce was invalid. Found: {rawNonce}, expected {nonce}");
			}

			return jwtSecurityToken;
		}

		/// <summary>
		///     Adds the open id configuration to the kernel to ensure that the signing keys for tokens are valid
		/// </summary>
		public static async Task<IServiceCollection> AddOpenIdConnectConfigurationAsync(this IServiceCollection serviceCollection)
		{
			var serviceProvider = serviceCollection.BuildServiceProvider();
			var configurationManager = await GetConfigurationManagerAsync(serviceProvider);
			serviceCollection.AddSingleton(configurationManager);

			var validationParameters = await GetTokenValidationParametersAsync(serviceProvider, CancellationToken.None);
			serviceCollection.AddSingleton(validationParameters);

			return serviceCollection;
		}

		private static async Task<ConfigurationManager<OpenIdConnectConfiguration>> GetConfigurationManagerAsync(IServiceProvider serviceProvider)
		{
			var keyVaultSecretProvider = serviceProvider.GetRequiredService<ISecretProvider>();

			var oktaIssuer = await keyVaultSecretProvider.GetSecretAsync("Okta-Issuer");

			var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
				oktaIssuer + "/.well-known/oauth-authorization-server",
				new PlatformOpenIdConnectConfigurationRetriever(),
				new PlatformHttpDocumentRetriever(serviceProvider));
			return configurationManager;
		}

		/// <summary>
		///     Adds the open id configuration to the kernel to ensure that the signing keys for tokens are valid
		/// </summary>
		public static IServiceCollection AddOpenIdConnectConfiguration(this IServiceCollection serviceCollection)
		{
			new JoinableTaskContext().Factory.Run(async () => await AddOpenIdConnectConfigurationAsync(serviceCollection));
			return serviceCollection;
		}

		/// <summary>
		///     Gets the validation settings which will be used for token validation
		/// </summary>
		public static async Task<TokenValidationParameters> GetTokenValidationParametersAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
		{
			var configurationManager = await GetConfigurationManagerAsync(serviceProvider);
			var keyVaultSecretProvider = serviceProvider.GetRequiredService<ISecretProvider>();

			var discoveryDocument = await configurationManager.GetConfigurationAsync(cancellationToken);
			var signingKeys = discoveryDocument.SigningKeys;

			var oktaIssuer = await keyVaultSecretProvider.GetSecretAsync("Okta-Issuer");
			var validAudience = await keyVaultSecretProvider.GetSecretAsync("Okta-ClientId");

			var validationParameters = new TokenValidationParameters
			{
				NameClaimType = "name",
				RoleClaimType = ClaimsPrincipalExtensions.RoleClaimName,
				RequireExpirationTime = true,
				RequireSignedTokens = true,
				ValidateIssuer = true,
				ValidIssuer = oktaIssuer,
				ValidateIssuerSigningKey = true,
				IssuerSigningKeys = signingKeys,
				ValidateLifetime = true,
				ValidateAudience = true,
				ValidAudience = validAudience,
				ClockSkew = TimeSpan.FromMinutes(value: 2) // Allow for some drift in server time
			};

			return validationParameters;
		}
	}
}