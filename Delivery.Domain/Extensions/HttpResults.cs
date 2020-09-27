using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Delivery.Domain.Extensions
{
    public static class HttpResults
    {

        public static ObjectResult InternalServerErrorResult(string errorMessage)
        {
            return new ObjectResult(errorMessage)
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            };
        }
        
        public static ObjectResult NotImplementedResult(string errorMessage = null)
        {
            return new ObjectResult(errorMessage)
            {
                StatusCode = StatusCodes.Status501NotImplemented,
            };
        }
    }
}