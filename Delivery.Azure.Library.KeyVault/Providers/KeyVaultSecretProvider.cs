using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Delivery.Azure.Library.Authentication.ActiveDirectory;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Configuration.Exceptions;
using Delivery.Azure.Library.Exceptions.Writers;
using Delivery.Azure.Library.KeyVault.Providers.Configurations;
using Delivery.Azure.Library.Resiliency.Stability.Enums;
using Delivery.Azure.Library.Resiliency.Stability.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Metrics;
using Delivery.Azure.Library.Telemetry.Constants;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.KeyVault.Providers
{
    /// <summary>
	///     Manages access to secrets stored in key vault.
	///     Dependencies:
	///     <see cref="Microsoft.Extensions.Configuration.IConfigurationProvider" />
	///     <see cref="IApplicationInsightsTelemetry" />
	///     Settings:
	///     <see cref="ConfigurationDefinition" />
	///     Feature Flags:
	///     FeatureFlag_FailoverKeyVaultToConfigurationProvider enables you to failover to the regular
	///     <see cref="Microsoft.Extensions.Configuration.IConfigurationProvider" /> where the secret will be loaded as a settings.
	/// </summary>
	public class KeyVaultSecretProvider : ISecretProvider
	{
		protected IServiceProvider ServiceProvider { get; }

		private static readonly HttpClient httpClient = new HttpClient
		{
			Timeout = TimeSpan.FromMinutes(value: 1)
		};

		protected virtual ActiveDirectoryAuthentication ActiveDirectoryAuthentication { get; }
		protected KeyVaultSecretConfigurationDefinition ConfigurationDefinition { get; }

		public KeyVaultSecretProvider(IServiceProvider serviceProvider) : this(serviceProvider, new KeyVaultSecretConfigurationDefinition(serviceProvider))
		{
		}

		public KeyVaultSecretProvider(IServiceProvider serviceProvider, KeyVaultSecretConfigurationDefinition configurationDefinition)
		{
			ServiceProvider = serviceProvider;
			ConfigurationDefinition = configurationDefinition;
			ActiveDirectoryAuthentication = new ActiveDirectoryAuthentication(serviceProvider);
		}

		public virtual async Task<string> GetSecretAsync(string secretName, string? shardKey = null)
		{
			try
			{
				var configurationProvider = ServiceProvider.GetRequiredService<IConfigurationProvider>();
				var secretValue = shardKey != null ? configurationProvider.GetSetting<string>($"{secretName}-{shardKey}", isMandatory: false):
					configurationProvider.GetSetting<string>($"{secretName}", isMandatory: false);

				if (string.IsNullOrEmpty(secretValue))
				{
					secretValue = configurationProvider.GetSetting<string>($"{secretName}", isMandatory: false);
				}

				if (!string.IsNullOrWhiteSpace(secretValue))
				{
					return secretValue;
				}

				KeyVaultClient keyVaultClient;
				if (ConfigurationDefinition.UseManagedServiceIdentity)
				{
					var azureServiceTokenProvider = new AzureServiceTokenProvider();
					keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
				}
				else
				{
					keyVaultClient = new KeyVaultClient(KeyVaultAuthenticationCallbackAsync, httpClient);
				}

				await using var stopwatch = ApplicationInsightsStopwatch.Start(ServiceProvider);
				var circuitBreaker = ServiceProvider.GetRequiredService<ICircuitManager>().GetCircuitBreaker(DependencyType.Api, ExternalDependency.KeyVault.ToString());

				if (shardKey != null)
				{
					var shardSecretExists = await circuitBreaker.CommunicateAsync(async () =>
						await keyVaultClient.GetSecretVersionsAsync(ConfigurationDefinition.VaultUri, $"{secretName}-{shardKey.ToLowerInvariant()}"));

					if (shardSecretExists.Any())
					{
						secretName = $"{secretName}-{shardKey.ToLowerInvariant()}";
					}
				}

				var secret = await circuitBreaker.CommunicateAsync(async () =>
					await keyVaultClient.GetSecretAsync(ConfigurationDefinition.VaultUri, secretName));

				stopwatch.Stop();
				stopwatch.TraceTotalElapsed("Retrieve Secret (Duration)");
				stopwatch.TraceTotalElapsed($"Retrieve Secret {secretName} (Duration)");

				ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Secret Retrieved", value: 1);
				return secret.Value;
			}
			catch (KeyVaultErrorException keyVaultErrorException)
			{
				var customProperties = new Dictionary<string, string>
				{
					{CustomProperties.FormattedException, keyVaultErrorException.WriteException()},
					{"SecretName", secretName}
				};

				var errorCode = Convert.ToString(keyVaultErrorException.Body?.Error?.Code?.ToLowerInvariant());
				ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"Error occurred while trying to retrieve secret '{secretName}' from {ConfigurationDefinition.VaultUri} (code: {errorCode}): {keyVaultErrorException.Message}", SeverityLevel.Warning, customProperties);

				switch (errorCode)
				{
					case "secretnotfound":
						ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Secret Not Found", value: 1);
						throw new SecretNotFoundException(secretName, keyVaultErrorException);
					case "forbidden":
						ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Unauthorized To Retrieve Secret", value: 1);
						throw new UnauthorizedAccessException($"Unauthorized to access secret '{secretName}'.", keyVaultErrorException);
					default:
						throw new KeyVaultErrorException($"Could not reach key vault at {ConfigurationDefinition.VaultUri} (error code: {errorCode}), Secret Name: {secretName}", keyVaultErrorException);
				}
			}
		}

		private async Task<string> KeyVaultAuthenticationCallbackAsync(string authority, string resource, string scope)
		{
			var accessToken = await ActiveDirectoryAuthentication.GetAppTokenAsync(authority, resource);
			return accessToken;
		}
		
	}
}