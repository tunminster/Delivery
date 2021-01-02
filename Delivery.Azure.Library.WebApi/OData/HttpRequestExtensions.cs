using Microsoft.AspNetCore.Http;

namespace Delivery.Azure.Library.WebApi.OData
{
    public static class HttpRequestExtensions
    {
        public static QueryableContract? GetQueryableContract(this HttpRequest httpRequest)
        {
            httpRequest.HttpContext.Items.TryGetValue(nameof(QueryableContract), out var item);

            return item is QueryableContract queryContract ? queryContract : null;
        }
    }
}