using System.Threading.Tasks;

namespace Delivery.Domain.FrameWork.Messages
{
    /// <summary>
    ///     The Message Publisher  is simple and powerful creating a one public method rule to enforce SRP
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IMessagePublisherAsync<in TMessage> : IMessagePublisher
    {
        Task PublishAsync(TMessage message);
    }

    /// <summary>
    ///     The Message Publisher with a result is against the grain in returning values but needed in many practical cases
    ///     TMessage is the concrete class that implements message. See IMessage.
    ///     TResult can be an integral type or reference type.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface IMessagePublisherAsync<in TMessage, TResult> : IMessagePublisher
    {
        public Task<TResult> PublishAsync(TMessage message);
    }
}