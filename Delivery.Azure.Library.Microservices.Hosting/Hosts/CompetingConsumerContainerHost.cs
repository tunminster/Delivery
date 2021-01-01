using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Collections;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Azure.Library.Exceptions.Writers;
using Delivery.Azure.Library.Messaging.Extensions;
using Delivery.Azure.Library.Messaging.Pump;
using Delivery.Azure.Library.Messaging.Pump.Arguments;
using Delivery.Azure.Library.Messaging.ServiceBus.Properties;
using Delivery.Azure.Library.Microservices.Hosting.Exceptions;
using Delivery.Azure.Library.Resiliency.Stability.Enums;
using Delivery.Azure.Library.Resiliency.Stability.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Metrics;
using Delivery.Azure.Library.Telemetry.Constants;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Delivery.Azure.Library.Microservices.Hosting.Hosts
{
	public abstract class CompetingConsumerContainerHost<TState> : ContainerHost
		where TState : Enum
	{
		private ServiceBusMessagePump? messagePump;

		protected ICircuitManager CircuitManager => ServiceProvider.GetRequiredService<ICircuitManager>();

		protected CompetingConsumerContainerHost(IHostBuilder hostBuilder) : base(hostBuilder)
		{
		}

		/// <summary>
		///     Name of the queue or topic that is being processed
		///     For topics the subscription name should be supplied
		/// </summary>
		public abstract string QueueOrTopicName { get; }

		/// <summary>
		///     Set to <c>True</c> for subscription to be created and destroyed by message pump
		/// </summary>
		/// <remarks>Useful for workloads where only one host should receive messages</remarks>
		protected virtual bool IsSubscriptionManagedByHost
		{
			get
			{
				// let testing in-memory subscriptions be created and cleaned up automatically
				var useInMemory = ServiceProvider.GetRequiredService<IConfigurationProvider>()
					.GetSettingOrDefault<bool>("Test_Use_In_Memory", defaultValue: false);
				return useInMemory;
			}
		}

		protected override async Task OnStartAsync(bool blockUntilCompletion = true)
		{
			messagePump = await ServiceBusMessagePump.CreateAsync(ServiceProvider, QueueOrTopicName, Ring,
				IsSubscriptionManagedByHost);
			messagePump.StartReceiving(HandleNewMessageArrivedAsync);

			ApplicationInsightsTelemetry.TrackTrace(
				$"Message pump started receiving messages on queue {QueueOrTopicName} for ring {Ring} (pump is managed by host: {IsSubscriptionManagedByHost})");
			await base.OnStartAsync(blockUntilCompletion);
		}

		protected override async Task OnStopAsync()
		{
			await base.OnStopAsync();

			if (messagePump != null)
			{
				await messagePump.StopAsync();
			}

			ApplicationInsightsTelemetry.TrackTrace($"Stopping message pump on queue {QueueOrTopicName}");
		}

		/// <summary>
		///     Provides the core extension point for containers to insert their message-processing logic.
		///     In order to track the processing state, all implementers should throw
		///     <see cref="StatefulMessageProcessingException{TState}" />.
		///     If you only want to preserve contextual information for the the telemetry, all implementers should throw
		///     <see cref="MessageProcessingException" />.
		/// </summary>
		/// <param name="message">The Service Bus message that was received</param>
		/// <param name="correlationId">The single correlation id which all telemetry should forward</param>
		/// <param name="stopwatch">The running stopwatch which will be used to benchmark message processing time</param>
		/// <param name="telemetryContextProperties">The telemetry properties which should be forwarded to the logger</param>
		protected abstract Task ProcessMessageAsync(Message message, string correlationId,
			ApplicationInsightsStopwatch stopwatch, Dictionary<string, string> telemetryContextProperties);

		private async Task HandleCompleteAsync(Message queuedMessage,
			Dictionary<string, string> customTelemetryProperties)
		{
			var circuitBreaker =
				CircuitManager.GetCircuitBreaker(DependencyType.Messaging, ExternalDependency.ServiceBus.ToString());
			await circuitBreaker.CommunicateAsync(async () =>
			{
				try
				{
					var correlationId = queuedMessage.CorrelationId;
					var lockToken = queuedMessage.GetLockToken();
					if (messagePump != null)
					{
						await messagePump.CompleteAsync(correlationId, lockToken);
					}
				}
				catch (MessageLockLostException messageLockLostException)
				{
					TraceMessageLockLost(queuedMessage, customTelemetryProperties, messageLockLostException);
				}
			});

			TraceCompletedMessage(queuedMessage.MessageId, customTelemetryProperties);
		}

		private async Task HandleExceptionAsync(Exception exception, Message message,
			Dictionary<string, string> customTelemetryProperties, Dictionary<string, object> messagePropertiesToModify)
		{
			ApplicationInsightsTelemetry.TrackException(exception, customTelemetryProperties);

			var circuitBreaker =
				CircuitManager.GetCircuitBreaker(DependencyType.Messaging, ExternalDependency.ServiceBus.ToString());
			await circuitBreaker.CommunicateAsync(async () =>
			{
				try
				{
					if (messagePump != null)
					{
						await messagePump.AbandonAsync(message, messagePropertiesToModify);
					}
				}
				catch (MessageLockLostException messageLockLostException)
				{
					TraceMessageLockLost(message, customTelemetryProperties, messageLockLostException);
				}
			});
		}

		private async Task HandleNewMessageArrivedAsync(NewMessageArrivedArguments newMessageArguments)
		{
			var messagePropertiesToModify = new Dictionary<string, object>();
			var telemetryContextProperties = new Dictionary<string, string>();
			telemetryContextProperties.AddRange(newMessageArguments.TelemetryContextProperties);

			var stopwatch = ApplicationInsightsStopwatch.Start(ServiceProvider);
			var enqueuedMessage = newMessageArguments.Message;

			try
			{
				try
				{
					TraceDequeuedMessage(enqueuedMessage.MessageId, telemetryContextProperties);

					await ProcessMessageAsync(enqueuedMessage, newMessageArguments.CorrelationId, stopwatch,
						telemetryContextProperties);

					await HandleCompleteAsync(enqueuedMessage, telemetryContextProperties);
				}
				catch (StatefulMessageProcessingException<TState> statefulMessageProcessingException)
				{
					telemetryContextProperties.AddRange(statefulMessageProcessingException.TelemetryContextProperties);

					telemetryContextProperties[MessageProperties.State] =
						statefulMessageProcessingException.CurrentState.ToString();
					messagePropertiesToModify[MessageProperties.State] =
						statefulMessageProcessingException.CurrentState.ToString();
					throw;
				}
				catch (MessageProcessingException messageProcessingException)
				{
					telemetryContextProperties.AddRange(messageProcessingException.TelemetryContextProperties);
					throw;
				}
				catch (ServiceBusCommunicationException serviceBusCommunicationException)
				{
					if (!serviceBusCommunicationException.Message.Contains("The connection was inactive"))
					{
						throw;
					}

					ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace(
						$"Connection closed due to inactivity: usually happens when a pod is being recreated. Exception: {serviceBusCommunicationException.Format()}",
						SeverityLevel.Warning);
				}
			}
			catch (Exception exception)
			{
				await HandleExceptionAsync(exception, enqueuedMessage, telemetryContextProperties,
					messagePropertiesToModify);
			}
			finally
			{
				stopwatch.TraceTotalElapsed($"{Assembly.GetEntryAssembly()?.GetName().Name} Processing Time");
			}
		}

		private void TraceDequeuedMessage(string messageId, Dictionary<string, string> telemetryContextProperties)
		{
			telemetryContextProperties[CustomProperties.QueueOrTopicName] = QueueOrTopicName;

			ApplicationInsightsTelemetry.TrackTrace($"Message '{messageId}' dequeued.",
				customProperties: telemetryContextProperties);
		}

		private void TraceCompletedMessage(string messageId, Dictionary<string, string> telemetryContextProperties)
		{
			telemetryContextProperties[CustomProperties.QueueOrTopicName] = QueueOrTopicName;

			ApplicationInsightsTelemetry.TrackTrace($"Message '{messageId}' processed successfully.",
				customProperties: telemetryContextProperties);
		}

		private void TraceMessageLockLost(Message queuedMessage, Dictionary<string, string> telemetryContextProperties,
			MessageLockLostException messageLockLostException)
		{
			// see https://social.technet.microsoft.com/wiki/contents/articles/5047.best-practices-for-leveraging-azure-service-bus-brokered-messaging.aspx
			telemetryContextProperties[CustomProperties.FormattedException] = messageLockLostException.WriteException();
			ApplicationInsightsTelemetry.TrackTrace($"Lock was lost on message {queuedMessage.MessageId}",
				customProperties: telemetryContextProperties);
		}
	}
}