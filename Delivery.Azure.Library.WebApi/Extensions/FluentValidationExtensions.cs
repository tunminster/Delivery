using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Validation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Azure.Library.WebApi.Extensions
{
    public static class FluentValidationExtensions
    {
        /// <summary>
        ///  Convert a string bad request to formatted type
        /// </summary>
        /// <param name="validationMessage"></param>
        /// <returns></returns>
        public static ObjectResult ConvertToBadRequest(this string validationMessage)
        {
            var badRequestValidation = new ValidationResult(new List<ValidationFailure> {new("", validationMessage)});
            var validationResult = badRequestValidation.ConvertToBadRequest();
            return validationResult;
        }
        
        public static ObjectResult ConvertToBadRequest(this ValidationResult validationResult)
        {
            if (validationResult.IsValid || !validationResult.Errors.Any())
            {
                throw new InvalidOperationException($"Expected to convert the validation result to a bad request, but the validation result is valid. Found: {validationResult.ConvertToJson()}");
            }

            var result = ConvertValidationResultToBadRequestContract(validationResult);

            var validationObjectResult = new ObjectResult(result)
            {
                StatusCode = (int) HttpStatusCode.BadRequest
            };

            return validationObjectResult;
        }
        
        public static BadRequestContract ConvertValidationResultToBadRequestContract(this ValidationResult validationResult)
        {
            var result = new BadRequestContract();
            foreach (var validationResultError in validationResult.Errors)
            {
                var badRequestErrorContract = new BadRequestErrorContract
                {
                    Type = RequestValidationStates.ValidationFailure.ToString(),
                    Message = validationResultError.ErrorMessage
                };

                result.Errors.Add(badRequestErrorContract);
            }

            return result;
        }
    }
}