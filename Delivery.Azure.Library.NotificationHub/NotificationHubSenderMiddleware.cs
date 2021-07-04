using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Delivery.Azure.Library.Exceptions.Traversers;
using Delivery.Azure.Library.NotificationHub.Connections;
using Delivery.Azure.Library.NotificationHub.Connections.Interfaces;
using Delivery.Azure.Library.Resiliency.Stability.Enums;
using Delivery.Azure.Library.Resiliency.Stability.Interfaces;
using Delivery.Azure.Library.Resiliency.Stability.Policies;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.NotificationHubs.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Polly.Retry;

namespace Delivery.Azure.Library.NotificationHub
{
    public class NotificationHubSenderMiddleware
    {
        private readonly IServiceProvider serviceProvider;
        
        protected NotificationHubSenderConnection NotificationHubSenderConnection { get; private set; }
        
        protected ICircuitBreaker CircuitBreaker { get; }
        
        public IConnectionMetadata ConnectionMetadata { get; }
        
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="serviceProvider">The kernel</param>
        /// <param name="notificationHubSenderConnection">Active Notification hub connection to use</param>
        protected NotificationHubSenderMiddleware(IServiceProvider serviceProvider, NotificationHubSenderConnection notificationHubSenderConnection)
        {
            this.serviceProvider = serviceProvider;
            CircuitBreaker = serviceProvider.GetRequiredService<ICircuitManager>().GetCircuitBreaker(DependencyType.Messaging, ExternalDependency.NotificationHub.ToString());

            NotificationHubSenderConnection = notificationHubSenderConnection;
            ConnectionMetadata = notificationHubSenderConnection.Metadata;
        }
        
        /// <summary>
        ///     Creates a <see cref="Task" /> to get a connection
        /// </summary>
        /// <param name="serviceProvider">The kernel</param>
        /// <param name="hubName">Name of the entity</param>
        /// <param name="connectionStringName">Name of the connection string to use</param>
        protected static async Task<NotificationHubSenderConnection> GetConnectionTaskAsync(IServiceProvider serviceProvider, string hubName, string connectionStringName)
        {
            var notificationHubConnectionManager = serviceProvider.GetRequiredService<INotificationHubSenderConnectionManager>();
            var connection = await notificationHubConnectionManager.GetConnectionAsync(hubName, connectionStringName);
            return connection;
        }
        
        /// <summary>
        ///     Allows to renew the connection in case of changes in the service bus
        /// </summary>
        private async Task RenewConnectionAsync()
        {
            var connectionManager = serviceProvider.GetRequiredService<INotificationHubSenderConnectionManager>();
            NotificationHubSenderConnection = await connectionManager.RenewConnectionAsync(ConnectionMetadata);
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