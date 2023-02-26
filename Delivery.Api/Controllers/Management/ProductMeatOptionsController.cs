using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.FrameWork.Context;
using Delivery.Domain.MeatOptions.Contracts.V1.RestContracts;
using Delivery.Domain.MeatOptions.Handlers.QueryHandlers;
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

        /// <summary>
        ///  Get meat option by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("get-meat-option", Order = 2)]
        [HttpPost]
        [ProducesResponseType(typeof(MeatOptionContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetMeatOptionAsync(string id)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var meatOptionQuery = new MeatOptionGetByIdQuery(id);
            var meatOptionContract = await new MeatOptionGetByIdQueryHandler(serviceProvider, executingRequestContextAdapter).HandleAsync(
                meatOptionQuery);

            return Ok(meatOptionContract);
        }
        
        /// <summary>
        ///  Get meat options by product id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("get-meat-options-by-product", Order = 2)]
        [HttpPost]
        [ProducesResponseType(typeof(List<MeatOptionContract>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetMeatOptionsByProductAsync(string id)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var meatOptionsByProductId = new MeatOptionsByProductId(id);
            var meatOptionContracts = await new MeatOptionsGetByProductQueryHandler(serviceProvider, executingRequestContextAdapter).HandleAsync(
                meatOptionsByProductId);

            return Ok(meatOptionContracts);
        }
    }
}