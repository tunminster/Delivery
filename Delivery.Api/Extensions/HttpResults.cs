using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Delivery.Api.Extensions
{
    public static  class HttpResults
    {
        public static ObjectResult InternalServerErrorResult([ActionResultObjectValue] string errorMessage)
        {
            return new ObjectResult(errorMessage)
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            };
        }
        
        public static ObjectResult NotImplementedResult([ActionResultObjectValue] string errorMessage = null)
        {
            return new ObjectResult(errorMessage)
            {
                StatusCode = StatusCodes.Status501NotImplemented,
            };
        }
    }
}