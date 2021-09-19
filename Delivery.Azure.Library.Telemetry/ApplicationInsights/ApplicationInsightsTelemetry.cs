using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Environments.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Collections;
using Delivery.Azure.Library.Core.Extensions.Http;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Azure.Library.Core.Guards;
using Delivery.Azure.Library.Exceptions.Writers;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Configurations;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Processors;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Telemetry;
using Delivery.Azure.Library.Telemetry.Constants;
using Delivery.Azure.Library.Telemetry.Stdout;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Metrics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Delivery.Azure.Library.Telemetry.ApplicationInsights
{
    /// <summary>
	///     Composes the <see cref="TelemetryClient" /> to provide a single place to create application insights telemetry
	///     Disabled in debug mode
	///     Dependencies:
	///     <see cref="IConfigurationProvider" />
	///     <see cref="IApplicationInsightsTelemetry" />
	///     <see cref="IEnvironmentProvider" />
	///     Settings:
	///     <see cref="ApplicationInsightsConfigurationDefinition" />
	/// </summary>
	public class ApplicationInsightsTelemetry : IApplicationInsightsTelemetry
	{
		private const int MaximumTelemetrySize = 8191;
		private readonly ApplicationInsightsConfigurationDefinition configurationDefinition;
		private readonly ILogger logger;

		private readonly TelemetryClient telemetryClient;

		public ApplicationInsightsTelemetry(IServiceProvider serviceProvider, string source, IEnumerable<ITelemetryInitializer>? telemetryInitializers = null) : this(serviceProvider, new ApplicationInsightsConfigurationDefinition(serviceProvider, source), telemetryInitializers)
		{
		}

		public ApplicationInsightsTelemetry(IServiceProvider serviceProvider, ApplicationInsightsConfigurationDefinition configurationDefinition, IEnumerable<ITelemetryInitializer>? telemetryInitializers = null)
		{
			this.configurationDefinition = configurationDefinition;

			logger = serviceProvider.GetService<ILogger?>() ?? new StdoutLogger(serviceProvider, configurationDefinition.Source).Logger;

			var telemetryConfiguration = CreateTelemetryConfiguration(configurationDefinition, telemetryInitializers);
			telemetryClient = new TelemetryClient(telemetryConfiguration)
			{
				InstrumentationKey = configurationDefinition.InstrumentationKey
			};
		}

		public virtual void Flush()
		{
			telemetryClient.Flush();
			Thread.Sleep(2 * 1000); // recommended flush pattern by application insights developers
		}

		public void TrackDependency(string dependencyName, MeasuredDependencyType measuredDependencyType, TimeSpan duration, string data, string dependencyTarget, bool? isSuccessful = null, Dictionary<string, string>? customProperties = null)
		{
			Guard.Against(measuredDependencyType == MeasuredDependencyType.None, nameof(measuredDependencyType));

			try
			{
				if (configurationDefinition.IsSourceDisabled)
				{
					return;
				}

				var rawDependencyType = DetermineRawDependencyType(measuredDependencyType);

				var dependencyTelemetry = new DependencyTelemetry
				{
					Name = dependencyName,
					Type = rawDependencyType,
					Duration = duration,
					Timestamp = DateTimeOffset.UtcNow,
					Data = data,
					Success = isSuccessful,
					Target = dependencyTarget
				};

				EnrichTelemetryProperties(customProperties, dependencyTelemetry);
				TrackDependency(dependencyTelemetry);
			}
			catch (Exception exception)
			{
				logger.LogError($"Failed to log a custom event. Reason: {exception}.");
			}
		}

		public void TrackEvent(string eventName, Dictionary<string, string>? customProperties = null)
		{
			try
			{
				if (configurationDefinition.IsSourceDisabled)
				{
					return;
				}

				var eventTelemetry = new EventTelemetry
				{
					Name = eventName
				};

				EnrichTelemetryProperties(customProperties, eventTelemetry);
				TrackEvent(eventTelemetry);
			}
			catch (Exception exception)
			{
				logger.LogError($"Failed to log a custom event. Reason: {exception}.");
			}
		}

		public void TrackException(Exception exception, Dictionary<string, string>? customProperties = null)
		{
			try
			{
				if (configurationDefinition.IsSourceDisabled)
				{
					return;
				}

				var exceptionTelemetry = new ExceptionTelemetry(exception)
				{
					Timestamp = DateTimeOffset.UtcNow
				};

				foreach (var dictionaryEntry in exception.Data)
				{
					if (dictionaryEntry is DictionaryEntry entry)
					{
						exceptionTelemetry.Properties.Add(entry.Key?.ToString() ?? string.Empty, entry.Value?.ToString());
					}
				}

				EnrichTelemetryProperties(customProperties, exceptionTelemetry);

				if (exceptionTelemetry.Properties is ConcurrentDictionary<string, string> concurrentExceptionProperties)
				{
					concurrentExceptionProperties.TryAdd(CustomProperties.FormattedException, exception.WriteException());
				}
				else
				{
					if (!exceptionTelemetry.Properties.ContainsKey(CustomProperties.FormattedException))
					{
						exceptionTelemetry.Properties.Add(CustomProperties.FormattedException, exception.WriteException());
					}
				}

				TrackException(exceptionTelemetry);
			}
			catch (Exception logException)
			{
				logger.LogError($"Failed to log exception with value '{exception}'. Reason: {logException}.");
			}
		}

		public void TrackMetric(string metricName, double value, Dictionary<string, string>? customProperties = null)
		{
			try
			{
				if (configurationDefinition.IsSourceDisabled)
				{
					return;
				}

				var metricTelemetry = new MetricIdentifier(metricName);
				AddDefaultCustomProperties(customProperties);
				TrackMetric(metricTelemetry, value);
			}
			catch (Exception exception)
			{
				logger.LogError($"Failed to log metric '{metricName}' with value '{value}'. Reason: {exception}.");
			}
		}

		public async Task TrackRequestAsync(HttpResponse response, TimeSpan duration = default, Dictionary<string, string>? customProperties = null)
		{
			try
			{
				if (configurationDefinition.IsSourceDisabled)
				{
					return;
				}

				var request = response.HttpContext.Request;
				var requestUrl = request.GetUri();

				var requestTelemetry = new RequestTelemetry
				{
					Url = requestUrl,
					ResponseCode = response.StatusCode.ToString(),
					Success = response.StatusCode < 400
				};

				// Provide a decent name for the request.
				// We've seen AI use names that refer to other requests
				if (requestUrl != null)
				{
					requestTelemetry.Name = $"{request.Method} {requestUrl.AbsolutePath}";
				}

				if (duration != default)
				{
					requestTelemetry.Duration = duration;
				}

				var properties = await GetGeneralInformationAsync(response);
				requestTelemetry.Properties.Merge(properties, OptimizeProperty);
				EnrichTelemetryProperties(customProperties, requestTelemetry);

				TrackRequest(requestTelemetry);
			}
			catch (Exception exception)
			{
				logger.LogError($"Failed to log a request. Reason: {exception}.");
			}
		}

		public async Task TrackRequestAsync(HttpResponseMessage response, TimeSpan duration = default, Dictionary<string, string>? customProperties = null)
		{
			try
			{
				if (configurationDefinition.IsSourceDisabled)
				{
					return;
				}

				var request = response.RequestMessage;

				var requestTelemetry = new RequestTelemetry
				{
					Url = request?.RequestUri,
					ResponseCode = response.StatusCode.ToString(),
					Success = response.IsSuccessStatusCode
				};

				// Provide a decent name for the request.
				// We've seen AI use names that refer to other requests
				if (request != null)
				{
					requestTelemetry.Name = $"{request.Method} {request.RequestUri?.AbsolutePath}";
				}

				if (duration != default)
				{
					requestTelemetry.Duration = duration;
				}

				var properties = await GetGeneralInformationAsync(response);
				requestTelemetry.Properties.Merge(properties, OptimizeProperty);
				EnrichTelemetryProperties(customProperties, requestTelemetry);

				TrackRequest(requestTelemetry);
			}
			catch (Exception exception)
			{
				logger.LogError($"Failed to log a request. Reason: {exception}.");
			}
		}

		public void TrackTrace(string message, SeverityLevel severityLevel = SeverityLevel.Information, Dictionary<string, string>? customProperties = null, string? sequence = null)
		{
			try
			{
				if (configurationDefinition.IsSourceDisabled)
				{
					return;
				}

				var traceTelemetry = new TraceTelemetry
				{
					SeverityLevel = severityLevel,
					Timestamp = DateTimeOffset.UtcNow,
					Message = message
				};

				if (!string.IsNullOrWhiteSpace(sequence))
				{
					traceTelemetry.Sequence = sequence;
				}

				EnrichTelemetryProperties(customProperties, traceTelemetry);
				TrackTrace(traceTelemetry);
			}
			catch (Exception exception)
			{
				logger.LogError($"Failed to trace '{message}'. Reason: {exception}.");
			}
		}

		protected virtual void TrackDependency(DependencyTelemetry dependencyTelemetry)
		{
			telemetryClient.TrackDependency(dependencyTelemetry);
		}

		protected virtual void TrackEvent(EventTelemetry eventTelemetry)
		{
			telemetryClient.TrackEvent(eventTelemetry);
		}

		protected virtual void TrackException(ExceptionTelemetry exceptionTelemetry)
		{
			telemetryClient.TrackException(exceptionTelemetry);
		}

		protected virtual void TrackMetric(MetricIdentifier metricTelemetry, double value)
		{
			telemetryClient.GetMetric(metricTelemetry).TrackValue(value);
		}

		protected virtual void TrackRequest(RequestTelemetry requestTelemetry)
		{
			telemetryClient.TrackRequest(requestTelemetry);
		}

		protected virtual void TrackTrace(TraceTelemetry traceTelemetry)
		{
			telemetryClient.TrackTrace(traceTelemetry);
		}

		private static TelemetryConfiguration CreateTelemetryConfiguration(ApplicationInsightsConfigurationDefinition configurationDefinition, IEnumerable<ITelemetryInitializer>? telemetryInitializers = null)
		{
			var telemetryConfiguration = new TelemetryConfiguration(configurationDefinition.InstrumentationKey);
			if (telemetryInitializers != null)
			{
				telemetryConfiguration.TelemetryInitializers.AddRange(telemetryInitializers);
			}

			telemetryConfiguration
				.TelemetryProcessorChainBuilder
				.Use(processor => new SampleSuccessfulDependenciesProcessor(processor, configurationDefinition.InitialSamplingPercentage))
				.Build();
			return telemetryConfiguration;
		}

		private static string DetermineRawDependencyType(MeasuredDependencyType measuredDependencyType)
		{
			switch (measuredDependencyType)
			{
				case MeasuredDependencyType.Availability:
					return "Availability";
				case MeasuredDependencyType.AzureBlob:
					return "Azure Blob";
				case MeasuredDependencyType.AzureCosmosDb:
					// Application Insights still uses the old name
					return "Azure DocumentDb";
				case MeasuredDependencyType.AzureEventHub:
					return "Azure Event Hubs";
				case MeasuredDependencyType.AzureQueue:
					return "Azure Queue";
				case MeasuredDependencyType.AzureServiceBus:
					return "Azure Service Bus";
				case MeasuredDependencyType.AzureStorage:
					return "Azure Storage";
				case MeasuredDependencyType.AzureTable:
					return "Azure Table";
				case MeasuredDependencyType.Sql:
					return "SQL";
				case MeasuredDependencyType.Http:
					return "Http";
				case MeasuredDependencyType.WebService:
					return "Web Service";
				case MeasuredDependencyType.Redis:
					return "Redis";
				// case MeasuredDependencyType.ElasticSearch:
				// 	return "ElasticSearch";
				case MeasuredDependencyType.Other:
					return "Http";
				default:
					throw new ArgumentOutOfRangeException(nameof(measuredDependencyType), measuredDependencyType, message: null);
			}
		}

		private static Dictionary<string, string> GetGeneralInformation(string httpVerb, Dictionary<string, string> requestHeaders, string rawRequestContent, Dictionary<string, string> responseHeaders, string rawResponseContent, string? correlationId)
		{
			var customProperties = new Dictionary<string, string>
			{
				{CustomProperties.CorrelationId, correlationId ?? string.Empty},
				{CustomProperties.RequestContent, rawRequestContent},
				{CustomProperties.ResponseContent, rawResponseContent},
				{CustomProperties.RequestVerb, httpVerb}
			};

			customProperties.Merge(requestHeaders);
			customProperties.Merge(responseHeaders);

			return customProperties;
		}

		private void AddDefaultCustomProperties(Dictionary<string, string>? customProperties = null)
		{
			if (customProperties != null)
			{
				customProperties["TelemetrySource"] = configurationDefinition.Source;
			}
		}

		private void AssignOperationId<TTelemetry>(TTelemetry traceTelemetry) where TTelemetry : ITelemetry, ISupportProperties
		{
			if (traceTelemetry.Properties == null || traceTelemetry.Context.Operation == null)
			{
				return;
			}

			if (traceTelemetry.Properties.TryGetValue(CustomProperties.CorrelationId, out var correlationId))
			{
				// ReSharper disable once PossibleNullReferenceException
				traceTelemetry.Context.Operation.Id = correlationId;
			}
		}

		private void EnrichTelemetryProperties<TTelemetryEntry>(Dictionary<string, string>? customProperties, TTelemetryEntry telemetryEntry) where TTelemetryEntry : ISupportProperties, ITelemetry
		{
			AddDefaultCustomProperties(customProperties);
			if (customProperties != null)
			{
				telemetryEntry.Properties.Merge(customProperties, OptimizeProperty);
			}

			AssignOperationId(telemetryEntry);
		}

		private async Task<Dictionary<string, string>> GetGeneralInformationAsync(HttpResponseMessage response)
		{
			// Correlation Id
			var request = response.RequestMessage;
			var correlationId = string.Empty;
			var trackedHttpRequestHeaders = new Dictionary<string, string>();
			var trackedHttpResponseHeaders = new Dictionary<string, string>();
			if (request?.Headers != null)
			{
				request.Headers.TryGetValues(CustomProperties.CorrelationId, out var correlationIds);
				if (correlationIds != null && correlationIds.Any())
				{
					correlationId = correlationIds.LastOrDefault();
				}

				// Request headers
				var httpRequestHeaders = request.Headers.AsDictionary();
				TrackRequestHttpHeaders(httpRequestHeaders, trackedHttpRequestHeaders);
			}

			// Request content
			var rawRequestContent = string.Empty;
			var requestContent = response.RequestMessage?.Content;
			if (requestContent != null)
			{
				var stream = await requestContent.ReadAsStreamAsync();
				using var streamReader = new StreamReader(stream);
				rawRequestContent = await streamReader.ReadToEndAsync();
				var requestContentHeaders = requestContent.Headers.AsDictionary();
				TrackRequestHttpHeaders(trackedHttpRequestHeaders, requestContentHeaders);
			}

			// Response Headers
			TrackResponseHttpHeaders(response.Headers.AsDictionary(), trackedHttpResponseHeaders);

			// Response Content
			var responseContent = response.Content;
			var responseStream = await responseContent.ReadAsStreamAsync();
			using var streamReaderResponse = new StreamReader(responseStream);
			string rawResponseContent = await streamReaderResponse.ReadToEndAsync();
			var responseContentHeaders = responseContent.Headers.AsDictionary();
			trackedHttpResponseHeaders.Merge(responseContentHeaders);

			// Http verb
			var httpVerb = request?.Method.Method ?? "Unknown";

			var customProperties = GetGeneralInformation(httpVerb, trackedHttpRequestHeaders, rawRequestContent, trackedHttpResponseHeaders, rawResponseContent, correlationId);

			return customProperties;
		}

		private async Task<Dictionary<string, string>> GetGeneralInformationAsync(HttpResponse response)
		{
			// Correlation Id
			var request = response.HttpContext.Request;
			var correlationId = request.GetCorrelationId();

			// Request content
			var maybeBody = await request.ReadBodyAsync();
			var rawRequestContent = maybeBody.IsPresent ? maybeBody.Value : $"<Body too large to be logged. See correlated telemetry for file details: {correlationId}>";

			// Request headers
			var trackedHttpRequestHeaders = new Dictionary<string, string>();
			var requestHttpHeaders = request.Headers.AsDictionary();
			requestHttpHeaders["Content-Type"] = request.ContentType;
			requestHttpHeaders["Content-Length"] = request.ContentLength?.ToString() ?? 0.ToString();
			TrackRequestHttpHeaders(requestHttpHeaders, trackedHttpRequestHeaders);

			// Response Headers
			var trackedHttpResponseHeaders = new Dictionary<string, string>();
			var responseHttpHeaders = response.Headers.AsDictionary();
			responseHttpHeaders["Content-Type"] = response.ContentType;
			responseHttpHeaders["Content-Length"] = response.ContentLength?.ToString() ?? 0.ToString();
			TrackResponseHttpHeaders(responseHttpHeaders, trackedHttpResponseHeaders);

			// Response Content
			var rawResponseContent = await response.ToRawContentAsync();

			// Http verb
			var httpVerb = request.Method;

			var customProperties = GetGeneralInformation(httpVerb, trackedHttpRequestHeaders, rawRequestContent, trackedHttpResponseHeaders, rawResponseContent, correlationId);
			return customProperties;
		}

		private KeyValuePair<string, string> OptimizeProperty(KeyValuePair<string, string> property)
		{
			var propertyName = property.Key;
			var propertyValue = property.Value;

			// Avoid going over the size limit for AI
			if (!string.IsNullOrWhiteSpace(propertyValue) && propertyValue.Length > MaximumTelemetrySize)
			{
				propertyValue = propertyValue.Substring(startIndex: 0, MaximumTelemetrySize);
			}

			// Avoid having spaces in the property name for processing reasons
			if (!string.IsNullOrWhiteSpace(propertyName))
			{
				propertyName = propertyName.Replace(" ", "");
			}

			return new KeyValuePair<string, string>(propertyName, propertyValue);
		}

		private void TrackHttpHeaders(string headerPrefix, Dictionary<string, string> httpHeaders, Dictionary<string, string> telemetryContextProperties)
		{
			foreach (var httpHeader in httpHeaders)
			{
				var isHeaderDisallowedForTelemetry = HttpHeadersExtensions.SensitiveHeaders.Any(sensitiveHeader => httpHeader.Key.ToLowerInvariant().Contains(sensitiveHeader.ToLowerInvariant()));
				if (!isHeaderDisallowedForTelemetry)
				{
					var httpHeaderPropertyName = $"{headerPrefix}-Header-{httpHeader.Key}";
					telemetryContextProperties[httpHeaderPropertyName] = httpHeader.Value.Format();
				}
			}
		}

		private void TrackRequestHttpHeaders(Dictionary<string, string> telemetryContextProperties, Dictionary<string, string> httpRequestHeaders)
		{
			TrackHttpHeaders("Request", telemetryContextProperties, httpRequestHeaders);
		}

		private void TrackResponseHttpHeaders(Dictionary<string, string> telemetryContextProperties, Dictionary<string, string> httpResponseHeaders)
		{
			TrackHttpHeaders("Response", httpResponseHeaders, telemetryContextProperties);
		}
	}
}