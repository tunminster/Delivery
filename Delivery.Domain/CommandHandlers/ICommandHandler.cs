using System.Threading.Tasks;

namespace Delivery.Domain.CommandHandlers
{
    public interface ICommandHandler<TCommand, TResult>
    {
        Task<TResult> HandleAsync(TCommand command);
    }
    
    public interface ICommandHandler<TCommand>
    {
        Task HandleAsync(TCommand command);
    }
}