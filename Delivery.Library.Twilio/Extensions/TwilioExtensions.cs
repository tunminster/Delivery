using System;
using System.Collections.Generic;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Sharding.Exceptions;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Azure.Library.Sharding.Sharding;
using Delivery.Library.Twilio.Contracts;
using Delivery.Library.Twilio.Properties;

namespace Delivery.Library.Twilio.Extensions
{
    public static class TwilioExtensions
    {
        public static TwilioEmailVerificationContract WithExecutingContext(
            this TwilioEmailVerificationContract twilioEmailVerificationContract,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            twilioEmailVerificationContract.UserProperties[ContractProperties.Shard] =
                executingRequestContextAdapter.GetShard().Key;
            twilioEmailVerificationContract.UserProperties[ContractProperties.Ring] =
                executingRequestContextAdapter.GetRing();

            twilioEmailVerificationContract.UserProperties[ContractProperties.CorrelationId] =
                executingRequestContextAdapter.GetCorrelationId();

            return twilioEmailVerificationContract;
        }
        
        /// <summary>
        ///  Get shard from contract header
        /// </summary>
        /// <param name="twilioEmailVerificationContract"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ShardNotFoundException"></exception>
        public static IShard GetShard(this TwilioEmailVerificationContract twilioEmailVerificationContract)
        {
            if (twilioEmailVerificationContract.UserProperties.TryGetValue(ContractProperties.Shard, out var shardKey))
            {
                var key = shardKey.ToString();

                if (key == null)
                {
                    throw new NotSupportedException($"{nameof(shardKey)} must be set. {twilioEmailVerificationContract.UserProperties.Format()}");
                }

                return Shard.Create(key);
            }

            throw new ShardNotFoundException();
        }
        
        
        /// <summary>
        ///     Gets the ring from the contract headers
        /// </summary>
        public static int? GetRing(this TwilioEmailVerificationContract twilioEmailVerificationContract)
        {
            if (twilioEmailVerificationContract.UserProperties.TryGetValue(ContractProperties.Ring, out var ring))
            {
                if (!int.TryParse(ring?.ToString(), out var ringInt))
                {
                    throw new InvalidOperationException($"{nameof(ring)} must be set with a value. {twilioEmailVerificationContract.UserProperties.Format()}");
                }

                return ringInt;
            }

            return null;
        }
        
        /// <summary>
        ///     Gets the correlation id from the contract headers
        /// </summary>
        public static string? GetCorrelationId(this TwilioEmailVerificationContract twilioEmailVerificationContract)
        {
            if (twilioEmailVerificationContract.UserProperties.TryGetValue(ContractProperties.CorrelationId, out var correlationId))
            {
                return correlationId.ToString();
            }

            return null;
        }
    }
}