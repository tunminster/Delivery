using System;
using System.Collections.Generic;
using System.Net.Mime;
using CloudNative.CloudEvents;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Collections;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Azure.Library.Messaging.Extensions;
using Delivery.Azure.Library.Messaging.Serialization.Enums;
using Delivery.Azure.Library.Messaging.ServiceBus.CloudEvents;
using Delivery.Azure.Library.Messaging.ServiceBus.Properties;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Sharding.Extensions;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Messaging.ServiceBus.Extensions
{
    public static class ServiceBusClientExtensions
	{
		public static Message CreateCloudEventMessage<TMessage>(this TMessage messageBody, IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter, string? source = null, MessageSerializerType messageSerializerType = MessageSerializerType.Json)
		{
			var webApiBaseUrl = source ?? serviceProvider.GetRequiredService<IConfigurationProvider>().GetSetting("Api-Base-Url");
			if (string.IsNullOrEmpty(webApiBaseUrl))
			{
				throw new NotSupportedException("Expected to find a source for the cloud event");
			}

			var messageId = Guid.NewGuid().ToString();
			var data = messageBody;
			var cloudEvent = new CloudEvent(typeof(TMessage).ToString(),
				new Uri(webApiBaseUrl),
				messageId,
				DateTime.UtcNow.ToUniversalTime(),
				new CloudEventMessageExtensions
				{
					MessageId = messageId,
					CorrelationId = executingRequestContextAdapter.GetCorrelationId(),
					Shard = executingRequestContextAdapter.GetShard().Key,
					Ring = executingRequestContextAdapter.GetRing()
				}
			)
			{
				DataContentType = new ContentType(MediaTypeNames.Application.Json),
				Data = data
			};

			var cloudEventBody = cloudEvent.Convert().ConvertToJsonBytes();
			var message = new Message(cloudEventBody);

			if (messageBody == null)
			{
				return message;
			}

			message.MessageId = messageId;
			message.ContentType = "application/json";
			message.UserProperties.Add(UserProperties.MessageType, messageBody.GetType().Name);
			message.UserProperties.Add(UserProperties.MessageSerializer, messageSerializerType.ToString());

			return message;
		}

		public static Message WithExecutingContext(this Message message, IExecutingRequestContextAdapter executingRequestContextAdapter)
		{
			message.WithRing(executingRequestContextAdapter.GetRing());
			message.WithShard(executingRequestContextAdapter.GetShard());
			message.WithCorrelationId(executingRequestContextAdapter.GetCorrelationId());
			return message;
		}

		public static Message WithRequestHeaders(this Message message, HttpRequest httpRequest)
		{
			var ring = httpRequest.GetRing();
			if (ring == null)
			{
				throw new InvalidOperationException($"No ring was supplied to the message: {httpRequest.Format()}");
			}

			message.WithRing(ring.Value);
			message.WithShard(httpRequest.GetShard());
			message.WithCorrelationId(httpRequest.GetCorrelationId());
			return message;
		}

		public static Message WithRing(this Message message, int ring)
		{
			message.UserProperties[MessageProperties.Ring] = ring;
			return message;
		}

		public static Message WithShard(this Message message, IShard shard)
		{
			message.UserProperties[MessageProperties.Shard] = shard.Key;
			return message;
		}

		public static Message WithCorrelationId(this Message message, string correlationId)
		{
			message.CorrelationId = correlationId;
			return message;
		}

		public static Message WithMessageProperties(this Message message, Dictionary<string, object> customProperties)
		{
			message.UserProperties.Merge(customProperties);
			return message;
		}

		public static Message WithTimeToLive(this Message message, TimeSpan timeToLive)
		{
			message.TimeToLive = timeToLive;
			return message;
		}

		public static Message WithScheduledEnqueueTimeUtc(this Message message, DateTimeOffset scheduledMessageDateTime)
		{
			message.ScheduledEnqueueTimeUtc = scheduledMessageDateTime.UtcDateTime;
			return message;
		}
	}
}