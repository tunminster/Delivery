using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.Resiliency.Stability.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Polly.Retry;

namespace Delivery.Azure.Library.Resiliency.Stability.Interfaces
{
    /// <summary>
	///     A circuit breaker opens/closes concurrent connections when an exception threshold has been exceeded to prevent
	///     remote services from being flooded with requests, allowing the system to recover and prevent failure
	/// </summary>
	public interface ICircuitBreaker
	{
		/// <summary>
		///     Specific category for that external dependency
		/// </summary>
		string Category { get; }

		/// <summary>
		///     Indication on what type of dependency we are relying on
		/// </summary>
		DependencyType DependencyType { get; }

		/// <summary>
		///     Duration in which the circuit breaker is open
		/// </summary>
		TimeSpan DurationOfBreak { get; }

		/// <summary>
		///     Indication on what dependency we rely on
		/// </summary>
		string ExternalDependency { get; }

		/// <summary>
		///     Amount of failed events before breaking the circuit
		/// </summary>
		int HandledEventsBeforeBreaking { get; }

		/// <summary>
		///     Communicates with the database by using the Circuit Breaker-principle
		/// </summary>
		/// <param name="communicationFunc">
		///     Function to communicate with the external dependency.
		/// </param>
		/// <param name="retryPolicy">Retry policy to use, if applicable</param>
		/// <param name="cancellationToken">Cancellation Token</param>
		/// <param name="retries">The number of times to retry. Important: use only for GET requests</param>
		/// <param name="retryWaitMilliseconds">The amount of time to wait in between retries</param>
		/// <param name="customTelemetryProperties">Information that will be used in telemetry</param>
		/// <returns>Result of the query</returns>
		Task CommunicateAsync(Func<Task> communicationFunc, AsyncRetryPolicy? retryPolicy = null, CancellationToken? cancellationToken = null, int? retries = null, int? retryWaitMilliseconds = null, Dictionary<string, string>? customTelemetryProperties = null);

		/// <summary>
		///     Communicates with the database by using the Circuit Breaker-principle
		/// </summary>
		/// <typeparam name="TResult">Type of expected result</typeparam>
		/// <param name="communicationFunc">
		///     Function to communicate with the database.
		/// </param>
		/// <param name="retryPolicy">Retry policy to use, if applicable</param>
		/// <param name="cancellationToken">Cancellation Token</param>
		/// <param name="retries">The number of times to retry. Important: use only for GET requests</param>
		/// <param name="retryWaitMilliseconds">The amount of time to wait in between retries</param>
		/// <param name="customTelemetryProperties">Information that will be used in telemetry</param>
		/// <returns>Result of the query</returns>
		Task<TResult> CommunicateAsync<TResult>(Func<Task<TResult>> communicationFunc, AsyncRetryPolicy? retryPolicy = null, CancellationToken? cancellationToken = null, int? retries = null, int? retryWaitMilliseconds = 200, Dictionary<string, string>? customTelemetryProperties = null);

		/// <summary>
		///     Communicates with an external Api by using the Circuit Breaker-principle
		/// </summary>
		/// <param name="communicationFunc">
		///     Function to communicate with the external Api. Note: function must create HttpRequestMessage in scope
		///     for retry logic
		/// </param>
		/// <param name="retryPolicy">Retry policy to use, if applicable</param>
		/// <param name="cancellationToken">Cancellation Token</param>
		/// <param name="retries">The number of times to retry. Important: use only for GET requests</param>
		/// <param name="retryWaitMilliseconds">The amount of time to wait in between retries</param>
		/// <param name="customTelemetryProperties">Information that will be used in telemetry</param>
		/// <returns>Response from the external Api</returns>
		Task<HttpResponseMessage> CommunicateWithApiAsync(Func<Task<HttpResponseMessage>> communicationFunc, AsyncRetryPolicy? retryPolicy = null, CancellationToken? cancellationToken = null, int? retries = null, int? retryWaitMilliseconds = null, Dictionary<string, string>? customTelemetryProperties = null);

		/// <summary>
		///     Communicates with an external Api by using the Circuit Breaker-principle
		/// </summary>
		/// <param name="communicationFunc">
		///     Function to communicate with the external Api. Note: function must create HttpRequestMessage in scope
		///     for retry logic
		/// </param>
		/// <param name="retryPolicy">Retry policy to use, if applicable</param>
		/// <param name="cancellationToken">Cancellation Token</param>
		/// <param name="retries">The number of times to retry. Important: use only for GET requests</param>
		/// <param name="retryWaitMilliseconds">The amount of time to wait in between retries</param>
		/// <param name="customTelemetryProperties">Information that will be used in telemetry</param>
		/// <returns>Response from the external Api</returns>
		Task<HttpResponse> CommunicateWithApiAsync(Func<Task<HttpResponse>> communicationFunc, AsyncRetryPolicy? retryPolicy = null, CancellationToken? cancellationToken = null, int? retries = null, int? retryWaitMilliseconds = null, Dictionary<string, string>? customTelemetryProperties = null);

		/// <summary>
		///     Communicates with Azure Service Bus by using the Circuit Breaker-principle
		/// </summary>
		/// <param name="communicationFunc">
		///     Function to communicate with Azure Service Bus. Note: function must create BrokeredMessage in scope
		///     for retry logic
		/// </param>
		/// <param name="retryPolicy">Retry policy to use, if applicable</param>
		/// <param name="cancellationToken">Cancellation Token</param>
		/// <param name="retries">The number of times to retry. Important: use only for GET requests</param>
		/// <param name="retryWaitMilliseconds">The amount of time to wait in between retries</param>
		/// <param name="customTelemetryProperties">Information that will be used in telemetry</param>
		/// <returns>Response from Azure Service Bus</returns>
		Task<Message> CommunicateWithServiceBusAsync(Func<Task<Message>> communicationFunc, AsyncRetryPolicy? retryPolicy = null, CancellationToken? cancellationToken = null, int? retries = null, int? retryWaitMilliseconds = null, Dictionary<string, string>? customTelemetryProperties = null);
	}
}