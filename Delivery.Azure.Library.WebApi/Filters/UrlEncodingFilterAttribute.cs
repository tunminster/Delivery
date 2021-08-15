using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Delivery.Azure.Library.WebApi.Filters
{
    /// <summary>
    ///     Repairs certain symbols which are lost during url encoding
    /// </summary>
    public class UrlEncodingFilterAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var query = context.HttpContext.Request.QueryString;

            if (query.HasValue)
            {
                // URL Encoded value for plus symbol is %2B, if you decode it you will get "+" symbol
                // But if you decode "+" symbol you will get space as a result.
                // Hence we need to replace + with %2B
                // Plus symbol has a special significance in URLs and hence not encoded by default

                var queryString = query.Value;
                var hex = Convert.ToByte(value: '+').ToString("X2");
                if (queryString != null)
                {
                    queryString = queryString.Replace("+", $"%{hex}");
                    context.HttpContext.Request.QueryString = new QueryString(queryString);
                }
            }

            await next();
        }
    }
}