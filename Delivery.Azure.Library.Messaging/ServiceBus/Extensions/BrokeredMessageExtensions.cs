using Microsoft.Azure.ServiceBus;

namespace Delivery.Azure.Library.Messaging.ServiceBus.Extensions
{
    public static class BrokeredMessageExtensions
    {
        /// <summary>
        ///     Gets the correlation Id for a <see cref="Message" />
        /// </summary>
        /// <param name="brokeredMessage">Message to determine the correlation id for</param>
        public static string GetCorrelationId(this Message brokeredMessage)
        {
            var correlationId = brokeredMessage.CorrelationId;
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = brokeredMessage.MessageId;
            }

            return correlationId;
        }
    }
}