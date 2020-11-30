namespace Delivery.Azure.Library.Messaging.ServiceBus.Properties
{
    /// <summary>
    ///     Message properties relevant for logging and telemetry
    /// </summary>
    public static class MessageProperties
    {
        /// <summary>
        ///     The last known state which the message is known to have successfully completed
        /// </summary>
        public const string State = "State";

        /// <summary>
        ///     The shard which the message applies to
        /// </summary>
        public const string Shard = "Shard";

        /// <summary>
        ///     The ring which the message is intended for
        /// </summary>
        public const string Ring = "Ring";
    }
}