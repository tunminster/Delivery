using System;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.FrameWork.Context;
using Delivery.Managements.Domain.Contracts.V1.MessageContracts.MeatOptions;
using Delivery.Managements.Domain.Contracts.V1.RestContracts.MeatOptions;
using Delivery.Managements.Domain.Converters;
using Delivery.Managements.Domain.Handlers.MessageHandler;
using Delivery.Managements.Domain.Validators.MeatOptions;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Management
{
    [Route("api/v1/management/product-meat-options" , Name = "12 - Meat option management")]
    [PlatformSwaggerCategory(ApiCategory.Management)]
    [ApiController]
    public class ProductMeatOptionsController : Controller
    {
        private readonly IServiceProvider serviceProvider;
        
        public ProductMeatOptionsController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///  Create meat option
        /// </summary>
        /// <returns></returns>
        [Route("create-meat-option", Order = 1)]
        [HttpPost]
        [ProducesResponseType(typeof(MeatOptionCreationStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateMeatOptionAsync(
            MeatOptionCreationContract meatOptionCreationContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult = await new MeatOptionCreationValidator().ValidateAsync(meatOptionCreationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var meatOptionCreationStatusContract = new MeatOptionCreationStatusContract
            {
                MeatOptionId = executingRequestContextAdapter.GetShard().GenerateExternalId(),
            };

            var meatOptionCreationMessageContract =
                meatOptionCreationContract.ConvertToMeatOptionCreationMessageContract(meatOptionCreationStatusContract
                    .MeatOptionId);

            var meatOptionCreationMessage = new MeatOptionCreationMessage
            {
                PayloadIn = meatOptionCreationMessageContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };

            await new MeatOptionCreationMessagePublisher(serviceProvider).PublishAsync(
                meatOptionCreationMessage);

            return Ok(meatOptionCreationStatusContract);
        }
    }
}