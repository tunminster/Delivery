using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading.Tasks;
using Delivery.Azure.Library.Authentication.ActiveDirectory.Caching.Tokens;
using Delivery.Azure.Library.Authentication.ActiveDirectory.Configurations;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Polly;

namespace Delivery.Azure.Library.Authentication.ActiveDirectory
{
    /// <summary>
	///     Provides a convenient method to create a <see cref="GraphServiceClient" />
	///     Dependencies:
	///     <see cref="IConfigurationProvider" />
	///     <see cref="IManagedCache" /> (optional; if set then the ActiveDirectoryClient token cache is based on
	///     <see cref="ActiveDirectoryManagedTokenCache" />)
	///     Settings:
	///     <see cref="ActiveDirectoryAuthenticationConfigurationDefinition" />
	/// </summary>
	public class ActiveDirectoryAuthentication : IAuthenticationProvider
	{
		private readonly ActiveDirectoryAuthenticationConfigurationDefinition authenticationConfigurationDefinition;

		public ActiveDirectoryAuthentication(IServiceProvider serviceProvider) : this(new ActiveDirectoryAuthenticationConfigurationDefinition(serviceProvider))
		{
		}

		public ActiveDirectoryAuthentication(ActiveDirectoryAuthenticationConfigurationDefinition authenticationConfigurationDefinition)
		{
			this.authenticationConfigurationDefinition = authenticationConfigurationDefinition;
		}

		/// <remarks>The in-built token cache is not recommended for productive use as it is not thread-safe</remarks>
		protected virtual TokenCache TokenCache => new ActiveDirectoryManagedTokenCache();

		public GraphServiceClient GetGraphServiceClient()
		{
			var graphServiceClient = new GraphServiceClient(this);
			return graphServiceClient;
		}

		public async Task AuthenticateRequestAsync(HttpRequestMessage request)
		{
			var accessToken = await GetAppTokenAsync();
			request.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
		}

		/// <summary>
		///     Gets an authentication token for a specific AD Application
		/// </summary>
		/// <param name="authority">Authority to which is being authentication</param>
		/// <param name="resource">Resource to acquire a token for</param>
		/// <returns>Authentication token</returns>
		public async Task<string> GetAppTokenAsync(string authority, string resource)
		{
			return await Policy.Handle<SocketException>()
				.WaitAndRetryAsync(retryCount: 15, _ => TimeSpan.FromSeconds(value: 1))
				.ExecuteAsync(async () =>
				{
					var authenticationContext = new AuthenticationContext(authority, TokenCache);
					var clientId = authenticationConfigurationDefinition.GetClientId();
					var appKey = authenticationConfigurationDefinition.GetAppKey();
					var clientCredential = new ClientCredential(clientId, appKey);

					var authenticationResult = await authenticationContext.AcquireTokenAsync(resource, clientCredential);
					return authenticationResult.AccessToken;
				});
		}

		/// <summary>
		///     Gets an authentication token for a specific AD Application
		/// </summary>
		/// <param name="resource">Resource to acquire a token for</param>
		/// <returns>Authentication token</returns>
		public async Task<string> GetAppTokenAsync(string resource)
		{
			var authority = authenticationConfigurationDefinition.GetAuthority();
			return await GetAppTokenAsync(authority, resource);
		}

		/// <summary>
		///     Gets an authentication token for a specific AD Application via the Graph API
		/// </summary>
		/// <returns>Authentication token</returns>
		public async Task<string> GetAppTokenAsync()
		{
			var authority = authenticationConfigurationDefinition.GetAuthority();
			return await GetAppTokenAsync(authority, authenticationConfigurationDefinition.GraphUrl);
		}
	}
}