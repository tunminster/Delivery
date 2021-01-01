using System;
using System.Collections.Generic;
using Delivery.Azure.Library.Messaging.ServiceBus.Properties;

namespace Delivery.Azure.Library.Microservices.Hosting.MessageHandlers.Factories
{
    public class MessageHandlerFactory
    {
        protected MessageHandlerFactory(string queueOrTopicName)
        {
            QueueOrTopicName = queueOrTopicName;
        }

        public string QueueOrTopicName { get; }

        /// <summary>
        ///     Gets the processing state from a list of properties based on the 'State' key
        /// </summary>
        /// <typeparam name="TState">Type of the expected processing state</typeparam>
        /// <param name="messageProperties">Properties to look for the state</param>
        protected TState GetProcessingState<TState>(IDictionary<string, object> messageProperties) where TState : Enum
        {
            return GetMessageProcessingState<TState>(messageProperties);
        }

        /// <summary>
        ///     Gets the processing state from a list of properties based on the 'State' key
        /// </summary>
        /// <typeparam name="TState">Type of the expected processing state</typeparam>
        /// <param name="messageProperties">Properties to look for the state</param>
        public static TState GetMessageProcessingState<TState>(IDictionary<string, object> messageProperties) where TState : Enum
        {
#pragma warning disable CS8653
            var messageProcessingState = default(TState);
#pragma warning restore CS8653

            if (messageProperties.TryGetValue(MessageProperties.State, out var potentialRawState))
            {
                messageProcessingState = (TState) Enum.Parse(typeof(TState), (string) potentialRawState);
            }
#pragma warning disable CS8603
            return messageProcessingState;
#pragma warning restore CS8603
        }
    }
}