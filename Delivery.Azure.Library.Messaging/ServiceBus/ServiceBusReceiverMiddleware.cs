using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Delivery.Azure.Library.Exceptions.Traversers;
using Delivery.Azure.Library.Messaging.ServiceBus.Connections;
using Delivery.Azure.Library.Messaging.ServiceBus.Connections.Interfaces;
using Delivery.Azure.Library.Resiliency.Stability.Enums;
using Delivery.Azure.Library.Resiliency.Stability.Interfaces;
using Delivery.Azure.Library.Resiliency.Stability.Policies;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Polly.Retry;

namespace Delivery.Azure.Library.Messaging.ServiceBus
{
    public class ServiceBusReceiverMiddleware
    {
        private readonly IServiceProvider serviceProvider;
		protected ServiceBusReceiverConnection ServiceBusConnection { get; private set; }
		protected ICircuitBreaker CircuitBreaker { get; }
		public IConnectionMetadata ConnectionMetadata { get; }

		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="serviceProvider">The kernel</param>
		/// <param name="serviceBusConnection">Active Service Bus connection to use</param>
		protected ServiceBusReceiverMiddleware(IServiceProvider serviceProvider, ServiceBusReceiverConnection serviceBusConnection)
		{
			this.serviceProvider = serviceProvider;
			CircuitBreaker = serviceProvider.GetRequiredService<ICircuitManager>().GetCircuitBreaker(DependencyType.Messaging, "ServiceBus");

			ServiceBusConnection = serviceBusConnection;
			ConnectionMetadata = serviceBusConnection.Metadata;
		}

		/// <summary>
		///     Creates a <see cref="Task" /> to get a connection
		/// </summary>
		/// <param name="serviceProvider">The kernel</param>
		/// <param name="entityName">Name of the entity</param>
		/// <param name="connectionStringName">Name of the connection string to use</param>
		protected static async Task<ServiceBusReceiverConnection> GetConnectionAsync(IServiceProvider serviceProvider, string entityName, string connectionStringName)
		{
			var serviceBusConnectionManager = serviceProvider.GetRequiredService<IServiceBusReceiverConnectionManager>();
			var connection = await serviceBusConnectionManager.GetConnectionAsync(entityName, connectionStringName);
			return connection;
		}

		/// <summary>
		///     Allows to renew the connection in case of changes in the service bus
		/// </summary>
		private async Task RenewConnectionAsync()
		{
			var connectionManager = serviceProvider.GetRequiredService<IServiceBusReceiverConnectionManager>();
			ServiceBusConnection = await connectionManager.RenewConnectionAsync(ConnectionMetadata);
		}

		/// <summary>
		///     Gets a retry policy that will renew the connection when it is needed
		/// </summary>
		protected AsyncRetryPolicy GetConnectionRenewalPolicy()
		{
			return RetryPolicyBuilder.Build(serviceProvider)
				.WithConnectionRenewalOn(exception =>
				{
					var isMessagingEntityNotFoundException = exception.GetExceptionOrInner<MessagingEntityNotFoundException>() != null;
					var isSocketException = exception.GetExceptionOrInner<SocketException>() != null;

					return isMessagingEntityNotFoundException || isSocketException;
				}, RenewConnectionAsync)
				.WithWaitAndRetry();
		}
    }
}