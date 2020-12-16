using System.Security.Claims;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;

namespace Delivery.Domain.Factories.Auth
{
    public interface IJwtFactory
    {
        Task<string> GenerateEncodedToken(string userName, ClaimsIdentity identity);
        ClaimsIdentity GenerateClaimsIdentity(string userName, string id, IExecutingRequestContextAdapter executingRequestContextAdapter);
    }
}