using System;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.FrameWork.Context;
using Delivery.Managements.Domain.Contracts.V1.MessageContracts.MeatOptionValues;
using Delivery.Managements.Domain.Contracts.V1.RestContracts.MeatOptionValues;
using Delivery.Managements.Domain.Handlers.MessageHandler;
using Delivery.Managements.Domain.Validators.MeatOptionValues;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Management
{
    [Route("api/v1/management/product-meat-option-values" , Name = "12 - Meat option values management")]
    [PlatformSwaggerCategory(ApiCategory.Management)]
    [ApiController]
    public class ProductMeatOptionValuesController : Controller
    {
        private readonly IServiceProvider serviceProvider;
        
        public ProductMeatOptionValuesController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///  Create meat option value
        /// </summary>
        /// <returns></returns>
        [Route("create-meat-option-value", Order = 1)]
        [HttpPost]
        [ProducesResponseType(typeof(HttpStatusCode), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateMeatOptionValueAsync(
            MeatOptionValueCreationContract meatOptionValueCreationContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult = await new MeatOptionValueCreationValidator().ValidateAsync(meatOptionValueCreationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var meatOptionValueCreationMessage = new MeatOptionValueCreationMessage
            {
                PayloadIn = meatOptionValueCreationContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };

            await new MeatOptionValueCreationMessagePublisher(serviceProvider).PublishAsync(
                meatOptionValueCreationMessage);

            return StatusCode((int) HttpStatusCode.Accepted);
        }
    }
}