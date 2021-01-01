using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Azure.Library.Messaging.Extensions;
using Delivery.Azure.Library.Messaging.Pump.Arguments;
using Delivery.Azure.Library.Messaging.ServiceBus;
using Delivery.Azure.Library.Messaging.ServiceBus.Connections;
using Delivery.Azure.Library.Messaging.ServiceBus.Extensions;
using Delivery.Azure.Library.Messaging.ServiceBus.Properties;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies.Models;
using Delivery.Azure.Library.Telemetry.Constants;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Messaging.Pump
{
    public class ServiceBusMessagePump : ServiceBusReceiverMiddleware
	{
		private const string NumberOfActiveMessagesPropertyName = "NumberOfActiveMessages";

		// Kubernetes waits for 30s (default) until force killing the process,
		// so here we wait 25s and give other code 5s to correctly shutdown.
		private readonly TimeSpan maxShutdownTime = TimeSpan.FromSeconds(value: 25);
		private readonly IServiceProvider serviceProvider;
		private bool isClosing;

		private int numberOfActiveMessages;

		protected ServiceBusMessagePump(IServiceProvider serviceProvider,
			ServiceBusReceiverConnection serviceBusConnection, string queueOrTopicName) : base(serviceProvider,
			serviceBusConnection)
		{
			QueueOrTopicName = queueOrTopicName;
			this.serviceProvider = serviceProvider;
		}

		/// <summary>
		///     Name of the queue that is being processed
		/// </summary>
		public string QueueOrTopicName { get; }

		private IApplicationInsightsTelemetry Telemetry =>
			serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>();

		/// <summary>
		///     Create a new instance of a message pump
		/// </summary>
		/// <param name="serviceProvider">The kernel</param>
		/// <param name="queueOrTopicName">Name of the queue or topic that will be processed</param>
		/// <param name="ring">The ring to listen to if set</param>
		/// <param name="isSubscriptionManagedByPump">Set to true to create and destroy subscription</param>
		public static async Task<ServiceBusMessagePump> CreateAsync(IServiceProvider serviceProvider,
			string queueOrTopicName, int? ring = null, bool isSubscriptionManagedByPump = false)
		{
			if (isSubscriptionManagedByPump && !ring.HasValue)
			{
				throw new InvalidOperationException($"{nameof(isSubscriptionManagedByPump)} is set to true but no subscription name was provided");
			}

			var topicName = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSetting("Topic_Name", isMandatory: false);
			var servicebusConnectionStringName = "ServiceBus-ConnectionString";

			if (!string.IsNullOrEmpty(topicName))
			{
				servicebusConnectionStringName = $"ServiceBus-Topic-{topicName}-ConnectionString";
			}

			var entityPath = queueOrTopicName;
			if (ring.HasValue)
			{
				var connectionString = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync(servicebusConnectionStringName);
				connectionString = ServiceBusReceiverConnectionManager.ReplaceEntityPath(connectionString);
				var managementClient = new ManagementClient(connectionString);
				var subscriptionName = GetSubscriptionName(ring);

				if (!await managementClient.SubscriptionExistsAsync(entityPath, subscriptionName))
				{
					if (!isSubscriptionManagedByPump)
					{
						throw new InvalidOperationException($"Subscription with ring '{ring}' and entity path '{entityPath}' should already exist");
					}

					entityPath = GetEntityPath(queueOrTopicName, ring.GetValueOrDefault());
					try
					{
						// create a topic subscription to make sure only this message pump can pick up these messages
						await CreateTopicSubscriptionAsync(connectionString, queueOrTopicName, ring.GetHashCode());
					}
					catch (MessagingEntityAlreadyExistsException)
					{
						// subscription already created
					}
				}
				else
				{
					entityPath = $"{queueOrTopicName}/subscriptions/{subscriptionName}";
				}
			}

			var connection = await GetConnectionAsync(serviceProvider, entityPath, servicebusConnectionStringName);
			return new ServiceBusMessagePump(serviceProvider, connection, queueOrTopicName);
		}

		/// <summary>
		///     Abandons message processing, allowing the message to be picked up by another consumer
		/// </summary>
		/// <param name="message">The message to abandon</param>
		/// <param name="propertiesToModify">
		///     Any additional message properties to set such as message state or information why the
		///     message was abandoned
		/// </param>
		public async Task AbandonAsync(Message message, IDictionary<string, object> propertiesToModify)
		{
			var dependencyName = GetDependencyName();
			var dependencyData = new DependencyData("Abandon");
			var dependencyTarget = GetDependencyTarget();

			var lockToken = message.GetLockToken();
			var correlationId = message.GetCorrelationId();

			await new DependencyMeasurement(serviceProvider)
				.ForDependency(dependencyName, MeasuredDependencyType.AzureServiceBus, dependencyData.ConvertToJson(), dependencyTarget)
				.WithCorrelationId(correlationId)
				.TrackAsync(async () => { await ServiceBusConnection.MessageReceiver.AbandonAsync(lockToken, propertiesToModify); });
		}

		/// <summary>
		///     Marks the queued message with a given lock token as completed
		/// </summary>
		/// <param name="correlationId">Correlation id of the message</param>
		/// <param name="lockToken">The token belonging to the message which will be completed</param>
		public async Task CompleteAsync(string correlationId, string lockToken)
		{
			var dependencyName = GetDependencyName();
			var dependencyData = new DependencyData("Complete");
			var dependencyTarget = GetDependencyTarget();

			await new DependencyMeasurement(serviceProvider)
				.ForDependency(dependencyName, MeasuredDependencyType.AzureServiceBus, dependencyData.ConvertToJson(),
					dependencyTarget)
				.WithCorrelationId(correlationId)
				.TrackAsync(async () => await ServiceBusConnection.MessageReceiver.CompleteAsync(lockToken));
		}

		/// <summary>
		///     Start receiving new messages
		/// </summary>
		/// <param name="newMessageAvailableFunc">Function to use in order to process new messages</param>
		public void StartReceiving(Func<NewMessageArrivedArguments, Task> newMessageAvailableFunc)
		{
			StartReceiving(newMessageAvailableFunc, maxConcurrentCalls: 10);
		}

		/// <summary>
		///     Start receiving new messages
		/// </summary>
		/// <param name="newMessageAvailableFunc">Function to use in order to process new messages</param>
		/// <param name="maxConcurrentCalls">Amount of maximum concurrent calls for processing messages in parallel</param>
		public void StartReceiving(Func<NewMessageArrivedArguments, Task> newMessageAvailableFunc,
			int maxConcurrentCalls)
		{
			var messageHandlerOptions = new MessageHandlerOptions(HandleMessageProcessingExceptionAsync)
			{
				AutoComplete = false,
				MaxConcurrentCalls = maxConcurrentCalls
			};

			try
			{
				async Task MessageHandler(Message message, CancellationToken cancellationToken)
				{
					if (isClosing)
					{
						// Do not accept and process new messages when shutting down.
						var properties = new Dictionary<string, object>();
						await AbandonAsync(message, properties);
						return;
					}

					var cycleId = Guid.NewGuid().ToString();
					var correlationId = message.GetCorrelationId();

					var telemetryContextProperties = new Dictionary<string, string>
					{
						{"Body", Encoding.UTF8.GetString(message.Body)},
						{CustomProperties.QueueOrTopicName, QueueOrTopicName},
						{CustomProperties.MessageId, message.MessageId},
						{CustomProperties.CorrelationId, correlationId},
						{CustomProperties.CycleId, cycleId},
						{CustomProperties.Shard, message.GetShard().Key},
						{CustomProperties.Ring, message.GetRing()?.ToString() ?? "Unknown"},
						{CustomProperties.DeliveryCount, message.SystemProperties.DeliveryCount.Format()},
						{
							CustomProperties.MessageEnqueuedTimeUtc,
							message.SystemProperties.EnqueuedTimeUtc.ToString("G")
						}
					};

					if (message.UserProperties.TryGetValue(UserProperties.MessageType, out var messageType))
					{
						telemetryContextProperties[UserProperties.MessageType] = messageType?.ToString() ?? "Unknown";
					}

					if (message.UserProperties.TryGetValue(MessageProperties.State, out var messageState))
					{
						telemetryContextProperties[MessageProperties.State] = messageState?.ToString() ?? "Unknown";
					}

					var newMessageAvailableArguments = new NewMessageArrivedArguments(message, correlationId, cycleId, telemetryContextProperties);

					try
					{
						Interlocked.Increment(ref numberOfActiveMessages);
						await newMessageAvailableFunc(newMessageAvailableArguments);
					}
					finally
					{
						Interlocked.Decrement(ref numberOfActiveMessages);
					}
				}

				ServiceBusConnection.MessageReceiver.RegisterMessageHandler(MessageHandler, messageHandlerOptions);
			}
			catch (InvalidOperationException invalidOperationException)
			{
				if (!invalidOperationException.Message.Contains("Can't create session when the connection is closing.") && !(invalidOperationException is ObjectDisposedException))
				{
					throw;
				}

				serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"Connection is closing issue encountered (is closing: {isClosing}). Exception: {invalidOperationException.Format()}", SeverityLevel.Warning);
			}
			catch (ServiceBusCommunicationException serviceBusCommunicationException)
			{
				if (!(serviceBusCommunicationException.InnerException is OperationCanceledException))
				{
					throw;
				}

				serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"Connection is closing issue encountered (is closing: {isClosing}). Exception: {serviceBusCommunicationException.Format()}", SeverityLevel.Warning);
			}
		}

		/// <summary>
		///     Stop processing messages
		/// </summary>
		public async Task StopAsync()
		{
			isClosing = true;

			await TryWaitUntilActiveMessagesAreProcessedAsync();

			await ServiceBusConnection.MessageReceiver.CloseAsync();

			WriteStoppedTelemetry();
		}

		private static async Task CreateTopicSubscriptionAsync(string connectionString, string topicName, int ring)
		{
			var subscriptionName = GetSubscriptionName(ring);
			var managementClient = new ManagementClient(connectionString);
			var subscriptionDescription = new SubscriptionDescription(topicName, subscriptionName)
			{
				// ensure that subscriptions clean themselves up after a period of inactivity as they are not required to last a long time
				// long-lived subscriptions should be created as infrastructure as code
				AutoDeleteOnIdle = TimeSpan.FromHours(value: 2),
				LockDuration = TimeSpan.FromSeconds(value: 30)
			};

			await managementClient.CreateSubscriptionAsync(subscriptionDescription);

			var subscriptionClient = new SubscriptionClient(connectionString, topicName, subscriptionName);
			var ruleDescription = new RuleDescription
			{
				Filter = new SqlFilter($"{nameof(MessageProperties.Ring)}={ring}"),
				Name = $"{nameof(MessageProperties.Ring)}Rule"
			};

			await subscriptionClient.RemoveRuleAsync("$Default");
			await subscriptionClient.AddRuleAsync(ruleDescription);
		}

		private static string GetEntityPath(string topicName, int ring)
		{
			var subscriptionName = GetSubscriptionName(ring);
			var entityPath = $"{topicName}/subscriptions/{subscriptionName}";
			return entityPath;
		}

		private static string GetSubscriptionName(int? ring)
		{
			var subscriptionName = $"ring-{ring}";
			return subscriptionName;
		}

		private string GetDependencyName()
		{
			return ServiceBusConnection.MessageReceiver.Path;
		}

		private string GetDependencyTarget()
		{
			return ServiceBusConnection.MessageReceiver.ServiceBusConnection.Endpoint.ToString();
		}

		private Task HandleMessageProcessingExceptionAsync(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
		{
			Telemetry.TrackException(exceptionReceivedEventArgs.Exception);
			return Task.CompletedTask;
		}

		private async Task TryWaitUntilActiveMessagesAreProcessedAsync()
		{
			var shutdownTime = DateTimeOffset.UtcNow;
			while (Interlocked.CompareExchange(ref numberOfActiveMessages, value: 0, comparand: 0) > 0 && DateTime.UtcNow - shutdownTime < maxShutdownTime)
			{
				await Task.Delay(millisecondsDelay: 1000);
			}
		}

		private void WriteStoppedTelemetry()
		{
			var telemetryProperties = new Dictionary<string, string>
			{
				{
					NumberOfActiveMessagesPropertyName,
					Interlocked.CompareExchange(ref numberOfActiveMessages, value: 0, comparand: 0).ToString()
				},
				{CustomProperties.QueueOrTopicName, QueueOrTopicName}
			};

			Telemetry.TrackTrace("Service bus message pump stopped", SeverityLevel.Information, telemetryProperties);
		}
	}
}