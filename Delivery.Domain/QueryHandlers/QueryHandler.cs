using System.Threading;
using System.Threading.Tasks;

namespace Delivery.Domain.QueryHandlers
{
    public abstract class QueryHandler<TQuery, TQueryResult>
        where TQuery : IQuery<TQueryResult>
        where TQueryResult : new()
    {
        public Task<TQueryResult> HandleAsync(TQuery request)
        {
            return HandleQueryAsync(request);
        }

        protected abstract Task<TQueryResult> HandleQueryAsync(TQuery request);

    }
}