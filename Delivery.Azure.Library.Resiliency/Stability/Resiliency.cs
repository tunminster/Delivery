using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Delivery.Azure.Library.Exceptions.Traversers;
using Microsoft.Azure.ServiceBus;
using Microsoft.WindowsAzure.Storage;
using Polly;

namespace Delivery.Azure.Library.Resiliency.Stability
{
    /// <summary>
	///     Provides a list of known exceptions which are safe to retry under general use
	/// </summary>
	public static class Resiliency
	{
		private static readonly List<ExceptionPredicate> databaseExceptions = new List<ExceptionPredicate>
		{
			exception => exception.GetExceptionOrInner<Win32Exception>()?.Message.Contains("The wait operation timed out") ?? false
		};

		public static readonly string ServerCouldNotCreateTransportLayerSecurityChannelExceptionMessage = "Could not create SSL/TLS secure channel";
		public static readonly string ServerProtocolViolationExceptionMessage = "The server committed a protocol violation";
		public static readonly string ResourceTemporarilyUnavailableExceptionMessage = "Resource temporarily unavailable";
		private static readonly string unableToConnectToServerExceptionMessage = "Unable to connect to the remote server";
		private static readonly string remoteHostClosedConnectionExceptionMessage = "The remote host closed the connection";
		private static readonly string connectionForciblyClosedByRemoteHostExceptionMessage = "An existing connection was forcibly closed by the remote host";
		private static readonly string azureOperationTimeout = "OperationTimedOut";
		private static readonly string azureServerBusyTimeout = "ServerBusy";

		private static readonly List<ExceptionPredicate> httpExceptions = new List<ExceptionPredicate>
		{
			exception =>
			{
				var httpRequestException = exception.GetExceptionOrInner<HttpRequestException>();
				if (httpRequestException == null)
				{
					return false;
				}

				return httpRequestException.Message.Contains("Bad Gateway");
			},
			exception => exception.GetExceptionOrInner<WebException>()?.Message.Contains(azureOperationTimeout) ?? false,
			exception => exception.GetExceptionOrInner<WebException>()?.Message.Contains(azureServerBusyTimeout) ?? false,
			exception => exception.GetExceptionOrInner<HttpRequestException>()?.Message.Contains(unableToConnectToServerExceptionMessage) ?? false,
			exception => exception.GetExceptionOrInner<HttpRequestException>()?.Message.Contains(ServerCouldNotCreateTransportLayerSecurityChannelExceptionMessage) ?? false,
			exception => exception.GetExceptionOrInner<HttpRequestException>()?.Message.Contains(ServerProtocolViolationExceptionMessage) ?? false,
			exception => exception.GetExceptionOrInner<HttpRequestException>()?.Message.Contains(ResourceTemporarilyUnavailableExceptionMessage) ?? false,
			exception => exception.GetExceptionOrInner<WebException>()?.Message.Contains(remoteHostClosedConnectionExceptionMessage) ?? false,
			exception => exception.GetExceptionOrInner<SocketException>()?.Message.Contains(connectionForciblyClosedByRemoteHostExceptionMessage) ?? false,
			exception =>
			{
				var webException = exception.GetExceptionOrInner<WebException>();
				if (webException == null)
				{
					return false;
				}

				return webException.Message.Contains(ServerProtocolViolationExceptionMessage) || webException.Message.Contains(ServerCouldNotCreateTransportLayerSecurityChannelExceptionMessage) || webException.Status == WebExceptionStatus.ConnectFailure || webException.Status == WebExceptionStatus.Timeout || webException.Status == WebExceptionStatus.ConnectionClosed ||
				       webException.Status == WebExceptionStatus.ServerProtocolViolation || webException.Status == WebExceptionStatus.ReceiveFailure;
			},
			exception => exception.GetExceptionOrInner<TaskCanceledException>()?.CancellationToken.IsCancellationRequested ?? false
		};

		private static readonly List<ExceptionPredicate> networkExceptions = new List<ExceptionPredicate>
		{
			exception => exception.GetExceptionOrInner<TimeoutException>() != null
		};

		private static readonly List<ExceptionPredicate> storageExceptions = new List<ExceptionPredicate>
		{
			exception => exception.GetExceptionOrInner<StorageException>()?.Message.Contains("503") ?? false,
			exception => exception.GetExceptionOrInner<StorageException>()?.Message.Contains("ServerBusy") ?? false,
			exception => exception.GetExceptionOrInner<StorageException>()?.Message.Contains("The client could not finish the operation within specified timeout") ?? false
		};

		private static readonly List<ExceptionPredicate> messagingExceptions = new List<ExceptionPredicate>
		{
			exception => exception.GetExceptionOrInner<ServiceBusException>()?.IsTransient == true
		};

		internal delegate bool ExceptionPredicate(Exception exception);

		/// <summary>
		///     Creates a default retry policy
		/// </summary>
		public static PolicyBuilder GetDefaultUnhealthyCriteria()
		{
			var policyBuilder = Policy.Handle<TimeoutException>();
			var retryExceptions = GetExceptionPredicates();

			foreach (var exceptionPredicate in retryExceptions)
			{
				policyBuilder.Or<Exception>(exceptionPredicate.Invoke);
			}

			return policyBuilder;
		}

		/// <summary>
		///     Creates a default criteria for resiliency
		/// </summary>
		/// <param name="unhealthyResultPredicate">Predicate to define a result as unhealthy</param>
		public static PolicyBuilder<TResult> GetDefaultUnhealthyCriteriaForResult<TResult>(Func<TResult, bool> unhealthyResultPredicate)
		{
			var policyBuilder = Policy.HandleResult(unhealthyResultPredicate);
			var retryExceptions = GetExceptionPredicates();

			foreach (var exceptionPredicate in retryExceptions)
			{
				policyBuilder.Or<Exception>(exceptionPredicate.Invoke);
			}

			return policyBuilder;
		}

		private static IEnumerable<ExceptionPredicate> GetExceptionPredicates()
		{
			var retryExceptions = new List<ExceptionPredicate>();
			retryExceptions.AddRange(networkExceptions);
			retryExceptions.AddRange(httpExceptions);
			retryExceptions.AddRange(databaseExceptions);
			retryExceptions.AddRange(storageExceptions);
			retryExceptions.AddRange(messagingExceptions);

			return retryExceptions;
		}
	}
}