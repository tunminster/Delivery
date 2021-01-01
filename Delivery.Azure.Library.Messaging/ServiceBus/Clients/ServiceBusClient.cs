using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Messaging.Extensions;
using Delivery.Azure.Library.Messaging.ServiceBus.Clients.Interfaces;
using Delivery.Azure.Library.Messaging.ServiceBus.Connections;
using Delivery.Azure.Library.Resiliency.Stability;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies.Models;
using Delivery.Azure.Library.Telemetry.Constants;
using Microsoft.Azure.ServiceBus;

namespace Delivery.Azure.Library.Messaging.ServiceBus.Clients
{
    public class ServiceBusClient : ServiceBusSenderMiddleware, IServiceBusClient
	{
		private readonly IServiceProvider serviceProvider;

		protected ServiceBusClient(IServiceProvider serviceProvider, ServiceBusSenderConnection serviceBusConnection) : base(serviceProvider, serviceBusConnection)
		{
			this.serviceProvider = serviceProvider;
		}

		/// <summary>
		///     Creates a new instance of the <see cref="ServiceBusClient" />
		/// </summary>
		/// <param name="serviceProvider">The kernel</param>
		/// <param name="entityName">Name of the entity</param>
		/// <param name="connectionStringName">Name of the connection string to use</param>
		public static async Task<ServiceBusClient> CreateAsync(IServiceProvider serviceProvider, string entityName, string connectionStringName)
		{
			var connection = await GetConnectionTaskAsync(serviceProvider, entityName, connectionStringName);

			return new ServiceBusClient(serviceProvider, connection);
		}

		public async Task<string> SendAsync(Message message)
		{
			var dependencyName = ServiceBusConnection.MessageSender.Path;
			var dependencyData = new DependencyData("Enqueue", message.MessageId);
			var dependencyTarget = ServiceBusConnection.MessageSender.ServiceBusConnection.Endpoint.ToString();

			var telemetryContextProperties = new Dictionary<string, string>
			{
				{"Body", Encoding.UTF8.GetString(message.Body)},
				{CustomProperties.MessageId, message.MessageId},
				{CustomProperties.CorrelationId, message.CorrelationId},
				{CustomProperties.Shard, message.GetShard().Key},
				{CustomProperties.Ring, message.GetRing()?.ToString() ?? "Unknown"}
			};

			var brokeredMessage = await new DependencyMeasurement(serviceProvider)
				.ForDependency(dependencyName, MeasuredDependencyType.AzureServiceBus, dependencyData.ConvertToJson(), dependencyTarget)
				.WithContextualInformation(telemetryContextProperties)
				.TrackAsync(async () =>
				{
					var sentMessage = await CircuitBreaker.CommunicateWithServiceBusAsync(async () =>
					{
						await ServiceBusConnection.MessageSender.SendAsync(message);
						return message;
					}, GetConnectionRenewalPolicy());

					return sentMessage;
				});

			return brokeredMessage.MessageId;
		}
	}
}