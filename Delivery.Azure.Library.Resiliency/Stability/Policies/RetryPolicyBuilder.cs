using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Configuration.Features.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Collections;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.FeatureFlags;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace Delivery.Azure.Library.Resiliency.Stability.Policies
{
    public class RetryPolicyBuilder
	{
		private readonly IServiceProvider serviceProvider;
		private IConfigurationProvider ConfigurationProvider => serviceProvider.GetRequiredService<IConfigurationProvider>();
		private IApplicationInsightsTelemetry Telemetry => serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>();
		private int DefaultRetryLimit => ConfigurationProvider.GetSettingOrDefault<int>("ResiliencyRetryDefaultLimit", defaultValue: 5);
		private int DefaultWaitLimitMilliseconds => ConfigurationProvider.GetSettingOrDefault<int>("ResiliencyWaitDefaultLimitMilliseconds", defaultValue: 1000);

		private readonly List<ConnectionRenewalDefinition> connectionRenewalDefinitions = new List<ConnectionRenewalDefinition>();

		public RetryPolicyBuilder(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		/// <summary>
		///     Builds the configured retry policy
		/// </summary>
		public static RetryPolicyBuilder Build(IServiceProvider serviceProvider)
		{
			return new RetryPolicyBuilder(serviceProvider);
		}

		/// <summary>
		///     Specifies that the connection needs to be renewed when a specific exception occurs
		/// </summary>
		/// <param name="unhealthyConnectionPredicate">Predicate to determine if a connection is unhealthy</param>
		/// <param name="connectionRenewalFunc">Allows to renew a connection</param>
		public RetryPolicyBuilder WithConnectionRenewalOn(Func<Exception, bool> unhealthyConnectionPredicate, Func<Task> connectionRenewalFunc)
		{
			var connectionRenewalDefinition = new ConnectionRenewalDefinition(unhealthyConnectionPredicate, connectionRenewalFunc);
			connectionRenewalDefinitions.Add(connectionRenewalDefinition);

			return this;
		}

		/// <summary>
		///     Specifies that it should wait a retry policy for a specified exception types
		/// </summary>
		/// <param name="retries">The maximum retries</param>
		/// <param name="millisecondsToWait">The time between retries</param>
		/// <param name="policyBuilder">Allows the caller to specify which exceptions to use</param>
		/// <param name="customTelemetryProperties">Information that will be used in telemetry</param>
		public AsyncRetryPolicy WithWaitAndRetry(int? retries = null, int? millisecondsToWait = null, PolicyBuilder? policyBuilder = null, Dictionary<string, string>? customTelemetryProperties = null)
		{
			retries = (retries ?? DefaultRetryLimit) + 1;
			millisecondsToWait ??= DefaultWaitLimitMilliseconds;
			policyBuilder ??= Resiliency.GetDefaultUnhealthyCriteria();
			customTelemetryProperties ??= new Dictionary<string, string>();

			if (connectionRenewalDefinitions.Any())
			{
				foreach (var connectionRenewalDefinition in connectionRenewalDefinitions)
				{
					policyBuilder.Or(connectionRenewalDefinition.HealthPredicate);
				}
			}

			return policyBuilder.WaitAndRetryAsync(retries.GetValueOrDefault(),
				retryCount => TimeSpan.FromSeconds(Math.Pow(x: 2, retryCount) / 2 * millisecondsToWait.Value / 1000.0),
				async (exception, span, executionCount, context) => { await RenewConnectionAsync(retries, customTelemetryProperties, context, executionCount, exception); });
		}

		private async Task RenewConnectionAsync(int? retries, Dictionary<string, string> customTelemetryProperties, Context context,
			int executionCount, Exception exception)
		{
			// Create a new collection as it could otherwise include information about another retry
			var featureProvider = serviceProvider.GetRequiredService<IFeatureProvider>();
			var retryContextTelemetryProperties = context.ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value.ToString() ?? string.Empty);
			retryContextTelemetryProperties.AddRange(customTelemetryProperties);

			if (await featureProvider.IsEnabledAsync(TelemetryFeatures.VerboseCircuitBreakerTelemetry.ToString()))
			{
				var traceMessage = $"Retrying after execution count {executionCount}. Exception: {exception}";
				serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace(traceMessage,
					SeverityLevel.Warning, retryContextTelemetryProperties);
			}

			if (connectionRenewalDefinitions.Count > 0)
			{
				foreach (var connectionRenewalDefinition in connectionRenewalDefinitions)
				{
					if (connectionRenewalDefinition == null)
					{
						throw new InvalidOperationException("No connection renewal function was provided");
					}

					if (connectionRenewalDefinition.HealthPredicate(exception))
					{
						await connectionRenewalDefinition.ConnectionRenewal();
						break;
					}
				}
			}

			if (executionCount == retries - 1)
			{
				throw exception;
			}

			Telemetry.TrackMetric("Retrying Execution", value: 1, retryContextTelemetryProperties);
		}
	}
}