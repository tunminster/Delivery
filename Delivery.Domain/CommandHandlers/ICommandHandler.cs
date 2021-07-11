using System.Threading.Tasks;

namespace Delivery.Domain.CommandHandlers
{
    public interface ICommandHandler<TCommand, TResult>
    {
        Task<TResult> Handle(TCommand command);
    }
    
    public interface ICommandHandler<TCommand>
    {
        Task Handle(TCommand command);
    }
}