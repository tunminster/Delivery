using System.Threading.Tasks;

namespace Delivery.Domain.QueryHandlers
{
    public interface IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        Task<TResult> Handle(TQuery query);
    }
}