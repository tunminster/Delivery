using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.Exceptions.Writers;
using Delivery.Azure.Library.Resiliency.Stability.Configurations;
using Delivery.Azure.Library.Resiliency.Stability.Enums;
using Delivery.Azure.Library.Resiliency.Stability.Interfaces;
using Delivery.Azure.Library.Resiliency.Stability.Policies;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Delivery.Azure.Library.Resiliency.Stability
{
    /// <summary>
	///     A circuit breaker implementation which has retry logic and application logging
	/// </summary>
	/// Dependencies:
	/// <see cref="IConfigurationProvider" />
	/// <see cref="IApplicationInsightsTelemetry" />
	/// Settings:
	/// <see cref="CircuitBreakerConfigurationDefinition" />
	public class CircuitBreaker : ICircuitBreaker
	{
		private readonly IServiceProvider serviceProvider;
		private readonly CircuitBreakerConfigurationDefinition configurationDefinition;

		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="serviceProvider">The kernel</param>
		/// <param name="dependencyType">Indication on what type of dependency we are relying on</param>
		/// <param name="externalDependency">Indication on what dependency we rely</param>
		/// <param name="category">Specific category for that external dependency</param>
		/// <param name="handledEventsBeforeBreaking">Number of handled events before breaking the circuit</param>
		/// <param name="durationOfBreak">Duration of in which the circuit is open</param>
		/// <param name="circuitBreakerConfigurationDefinition">The settings to use for the circuit breaker</param>
		public CircuitBreaker(IServiceProvider serviceProvider, DependencyType dependencyType, string externalDependency, string category, int handledEventsBeforeBreaking, TimeSpan durationOfBreak, CircuitBreakerConfigurationDefinition? circuitBreakerConfigurationDefinition = null)
		{
			this.serviceProvider = serviceProvider;
			Category = category;
			DependencyType = dependencyType;
			DurationOfBreak = durationOfBreak;
			ExternalDependency = externalDependency;
			HandledEventsBeforeBreaking = handledEventsBeforeBreaking;

			configurationDefinition = circuitBreakerConfigurationDefinition ?? new CircuitBreakerConfigurationDefinition(serviceProvider);
		}

		/// <summary>
		///     Creates the default circuit breaker policy that will be used
		/// </summary>
		/// <typeparam name="TResult">Type of expected result</typeparam>
		/// <param name="handledEventsBeforeBreaking">Number of handled events before breaking the circuit</param>
		/// <param name="durationOfBreak">Duration of in which the circuit is open</param>
		/// <param name="unhealthyResultPredicate">Predicate to define a result as unhealthy</param>
		protected virtual AsyncCircuitBreakerPolicy<TResult> GetDefaultCircuitBreakerPolicyWithResult<TResult>(int handledEventsBeforeBreaking, TimeSpan durationOfBreak, Func<TResult, bool> unhealthyResultPredicate)
		{
			var defaultPolicy = Resiliency.GetDefaultUnhealthyCriteriaForResult(unhealthyResultPredicate);

			var policy = defaultPolicy.CircuitBreakerAsync(handledEventsBeforeBreaking, durationOfBreak, OnCircuitBroken, OnCircuitReset, OnCircuitHalfOpen);

			return policy;
		}

		/// <summary>
		///     Creates the default circuit breaker policy that will be used
		/// </summary>
		/// <param name="handledEventsBeforeBreaking">Number of handled events before breaking the circuit</param>
		/// <param name="durationOfBreak">Duration of in which the circuit is open</param>
		/// <param name="unhealthyPredicate">Predicate to define a unhealthy result</param>
		protected virtual AsyncCircuitBreakerPolicy GetDefaultCircuitBreakerPolicy(int handledEventsBeforeBreaking, TimeSpan durationOfBreak, Func<Exception, bool> unhealthyPredicate)
		{
			var defaultPolicy = Resiliency.GetDefaultUnhealthyCriteria();
			defaultPolicy.Or(unhealthyPredicate);

			var policy = defaultPolicy.CircuitBreakerAsync(handledEventsBeforeBreaking, durationOfBreak, OnCircuitBroken, OnCircuitReset, OnCircuitHalfOpen);

			return policy;
		}

		public string Category { get; }

		public TimeSpan DurationOfBreak { get; }

		public DependencyType DependencyType { get; }

		public int HandledEventsBeforeBreaking { get; }

		public string ExternalDependency { get; }

		public async Task<TResult> CommunicateAsync<TResult>(Func<Task<TResult>> communicationFunc, AsyncRetryPolicy? retryPolicy = null, CancellationToken? cancellationToken = null, int? retries = null, int? retryWaitMilliseconds = 200, Dictionary<string, string>? customTelemetryProperties = null)
		{
			retries ??= configurationDefinition.DefaultRetryLimit;

			customTelemetryProperties = EnrichTelemetry(DependencyType.None, customTelemetryProperties);

			bool UnhealthyPredicate(Exception exception) => exception is TimeoutException;

			return await ExecuteWithResultAndRetryOnExceptionAsync(communicationFunc, UnhealthyPredicate, retryPolicy, retries, customTelemetryProperties: customTelemetryProperties);
		}

		public async Task CommunicateAsync(Func<Task> communicationFunc, AsyncRetryPolicy? retryPolicy = null, CancellationToken? cancellationToken = null, int? retries = null, int? retryWaitMilliseconds = null, Dictionary<string, string>? customTelemetryProperties = null)
		{
			retries ??= configurationDefinition.DefaultRetryLimit;

			customTelemetryProperties = EnrichTelemetry(DependencyType.None, customTelemetryProperties);

			bool UnhealthyPredicate(Exception exception) => exception is TimeoutException;

			await ExecuteAsync(communicationFunc, UnhealthyPredicate, retryPolicy, retries, customTelemetryProperties: customTelemetryProperties);
		}

		public async Task<HttpResponseMessage> CommunicateWithApiAsync(Func<Task<HttpResponseMessage>> communicationFunc, AsyncRetryPolicy? retryPolicy = null, CancellationToken? cancellationToken = null, int? retries = null, int? retryWaitMilliseconds = null, Dictionary<string, string>? customTelemetryProperties = null)
		{
			retries ??= configurationDefinition.DefaultRetryLimit;

			customTelemetryProperties = EnrichTelemetry(DependencyType.Api, customTelemetryProperties);

			bool UnhealthyResultPredicate(HttpResponseMessage response) => response.StatusCode == HttpStatusCode.RequestTimeout || response.StatusCode == HttpStatusCode.ServiceUnavailable || response.StatusCode == HttpStatusCode.BadGateway;

			return await ExecuteWithResultAsync(communicationFunc, UnhealthyResultPredicate, retryPolicy, retries, customTelemetryProperties: customTelemetryProperties);
		}

		public async Task<HttpResponse> CommunicateWithApiAsync(Func<Task<HttpResponse>> communicationFunc, AsyncRetryPolicy? retryPolicy = null, CancellationToken? cancellationToken = null, int? retries = null, int? retryWaitMilliseconds = null, Dictionary<string, string>? customTelemetryProperties = null)
		{
			retries ??= configurationDefinition.DefaultRetryLimit;

			customTelemetryProperties = EnrichTelemetry(DependencyType.Api, customTelemetryProperties);

			bool UnhealthyResultPredicate(HttpResponse response) => response.StatusCode == (int) HttpStatusCode.RequestTimeout || response.StatusCode == (int) HttpStatusCode.ServiceUnavailable || response.StatusCode == (int) HttpStatusCode.BadGateway;

			return await ExecuteWithResultAsync(communicationFunc, UnhealthyResultPredicate, retryPolicy, retries, customTelemetryProperties: customTelemetryProperties);
		}

		public async Task<Message> CommunicateWithServiceBusAsync(Func<Task<Message>> communicationFunc, AsyncRetryPolicy? retryPolicy = null, CancellationToken? cancellationToken = null, int? retries = null, int? retryWaitMilliseconds = null, Dictionary<string, string>? customTelemetryProperties = null)
		{
			retries ??= configurationDefinition.DefaultRetryLimit;
			bool UnhealthyResultPredicate(Message message) => false;

			customTelemetryProperties = EnrichTelemetry(DependencyType.Messaging, customTelemetryProperties);

			return await ExecuteWithResultAsync(communicationFunc, UnhealthyResultPredicate, retryPolicy, retries, customTelemetryProperties: customTelemetryProperties);
		}
		
		public async Task<string> CommunicateWithNotificationHubAsync(Func<Task<string>> communicationFunc, AsyncRetryPolicy? retryPolicy = null, CancellationToken? cancellationToken = null, int? retries = null, int? retryWaitMilliseconds = null, Dictionary<string, string>? customTelemetryProperties = null)
		{
			retries ??= configurationDefinition.DefaultRetryLimit;
			bool UnhealthyResultPredicate(string result) => false;

			customTelemetryProperties = EnrichTelemetry(DependencyType.Messaging, customTelemetryProperties);

			return await ExecuteWithResultAsync(communicationFunc, UnhealthyResultPredicate, retryPolicy, retries, customTelemetryProperties: customTelemetryProperties);
		}

		private async Task ExecuteAsync(Func<Task> communicationFunc, Func<Exception, bool> unhealthyResultPredicate, AsyncRetryPolicy? retryPolicy = null, int? retries = null, int? retryWaitMilliseconds = 200, Dictionary<string, string>? customTelemetryProperties = null)
		{
			retries ??= configurationDefinition.DefaultRetryLimit;
			retryPolicy ??= RetryPolicyBuilder.Build(serviceProvider).WithWaitAndRetry(retries, retryWaitMilliseconds, customTelemetryProperties: customTelemetryProperties);

			var circuitBreakerPolicy = GetDefaultCircuitBreakerPolicy(HandledEventsBeforeBreaking, DurationOfBreak, unhealthyResultPredicate);

			await retryPolicy.ExecuteAsync(async () => await circuitBreakerPolicy.ExecuteAsync(communicationFunc));
		}

		private async Task<TResult> ExecuteWithResultAndRetryOnExceptionAsync<TResult>(Func<Task<TResult>> communicationFunc, Func<Exception, bool> unhealthyPredicate, AsyncRetryPolicy? retryPolicy = null, int? retries = null, int? retryWaitMilliseconds = 200, Dictionary<string, string>? customTelemetryProperties = null)
		{
			try
			{
				retries ??= configurationDefinition.DefaultRetryLimit;
				retryPolicy ??= RetryPolicyBuilder.Build(serviceProvider).WithWaitAndRetry(retries, retryWaitMilliseconds, customTelemetryProperties: customTelemetryProperties);

				var circuitBreakerPolicy = GetDefaultCircuitBreakerPolicy(HandledEventsBeforeBreaking, DurationOfBreak, unhealthyPredicate);

				return await retryPolicy.ExecuteAsync(async () => await circuitBreakerPolicy.ExecuteAsync(communicationFunc));
			}
			catch (BrokenCircuitException<TResult> circuitException)
			{
				return circuitException.Result;
			}
		}

		private async Task<TResult> ExecuteWithResultAsync<TResult>(Func<Task<TResult>> communicationFunc, Func<TResult, bool> unhealthyResultPredicate, AsyncRetryPolicy? retryPolicy = null, int? retries = null, int? retryWaitMilliseconds = 200, Dictionary<string, string>? customTelemetryProperties = null)
		{
			try
			{
				retries ??= configurationDefinition.DefaultRetryLimit;
				retryPolicy ??= RetryPolicyBuilder.Build(serviceProvider).WithWaitAndRetry(retries, retryWaitMilliseconds, customTelemetryProperties: customTelemetryProperties);

				var circuitBreakerPolicy = GetDefaultCircuitBreakerPolicyWithResult(HandledEventsBeforeBreaking, DurationOfBreak, unhealthyResultPredicate);

				return await retryPolicy.ExecuteAsync(async () => await circuitBreakerPolicy.ExecuteAsync(communicationFunc));
			}
			catch (BrokenCircuitException<TResult> circuitException)
			{
				return circuitException.Result;
			}
		}

		private Dictionary<string, string> EnrichTelemetry(DependencyType dependencyType, Dictionary<string, string>? customTelemetryProperties)
		{
			customTelemetryProperties ??= new Dictionary<string, string>
			{
				[CustomProperties.ExternalDependency] = ExternalDependency, [CustomProperties.DependencyType] = dependencyType.ToString()
			};

			return customTelemetryProperties;
		}

		private void LogStateChange(CircuitState circuitState, Exception? exception = null)
		{
			var customProperties = new Dictionary<string, string>
			{
				{CustomProperties.ExternalDependency, ExternalDependency},
				{CustomProperties.Category, Category},
				{"DependencyType", DependencyType.ToString()}
			};

			if (exception != null)
			{
				customProperties.Add(CustomProperties.FormattedException, exception.WriteException());
			}

			serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"Circuit breaker went to {circuitState}-state.", customProperties: customProperties);
		}

		private void OnCircuitBroken<TResult>(DelegateResult<TResult> result, TimeSpan duration)
		{
			LogStateChange(CircuitState.Open, result.Exception);
		}

		private void OnCircuitBroken(Exception exception, TimeSpan duration)
		{
			LogStateChange(CircuitState.Open, exception);
		}

		private void OnCircuitReset()
		{
			LogStateChange(CircuitState.Closed);
		}

		private void OnCircuitHalfOpen()
		{
			LogStateChange(CircuitState.HalfOpen);
		}
	}
}