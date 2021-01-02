using System;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Azure.Library.Messaging.ServiceBus.Properties;
using Delivery.Azure.Library.Sharding.Exceptions;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Azure.Library.Sharding.Sharding;
using Microsoft.Azure.ServiceBus;
using static System.Enum;

namespace Delivery.Azure.Library.Messaging.Extensions
{
    public static class MessageExtensions
    {
        /// <summary>
        ///     Gets the lock token for a specific message
        /// </summary>
        /// <param name="message">Message to retrieve the lock token for</param>
        public static string GetLockToken(this Message message)
        {
            return message.SystemProperties.LockToken;
        }

        /// <summary>
        ///     Gets the processing state
        /// </summary>
        /// <param name="message">Message to determine processing state for</param>
        public static TState GetMessageProcessingState<TState>(this Message message)
            where TState : Enum
        {
            if (message.UserProperties.TryGetValue(MessageProperties.State, out var potentialRawState))
            {
                return (TState) Parse(typeof(TState), (string) potentialRawState);
            }

#pragma warning disable CS8603
#pragma warning disable CS8653
            return default;
#pragma warning restore CS8653
#pragma warning restore CS8603
        }

        /// <summary>
        ///     Gets the shard from the message headers
        /// </summary>
        /// <param name="message">Message to determine shard for</param>
        public static IShard GetShard(this Message message)
        {
            if (message.UserProperties.TryGetValue(MessageProperties.Shard, out var shardKey))
            {
                var key = shardKey.ToString();

                if (key == null)
                {
                    throw new NotSupportedException($"{nameof(shardKey)} must be set. {message.UserProperties.Format()}");
                }

                return Shard.Create(key);
            }

            throw new ShardNotFoundException();
        }

        /// <summary>
        ///     Gets the ring from the message headers
        /// </summary>
        /// <param name="message">Message to determine ring for</param>
        public static int? GetRing(this Message message)
        {
            if (message.UserProperties.TryGetValue(MessageProperties.Ring, out var ring))
            {
                if (!int.TryParse(ring?.ToString(), out var ringInt))
                {
                    throw new InvalidOperationException($"{nameof(ring)} must be set with a value. {message.UserProperties.Format()}");
                }

                return ringInt;
            }

            return null;
        }
    }
}