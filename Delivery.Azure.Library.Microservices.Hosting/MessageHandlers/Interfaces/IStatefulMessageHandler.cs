using System;

namespace Delivery.Azure.Library.Microservices.Hosting.MessageHandlers.Interfaces
{
    public interface IStatefulMessageHandler<out TState> : IMessageHandler
        where TState : Enum
    {
        /// <summary>
        ///     Current state of the processing
        /// </summary>
        TState LastProcessedState { get; }
    }
}