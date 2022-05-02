using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.FrameWork.Context;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreTypeCreations;
using Delivery.Store.Domain.Handlers.QueryHandlers.StoreTypeGetQueries;
using Delivery.Store.Domain.Services.ApplicationServices.StoreCreations;
using Delivery.Store.Domain.Services.ApplicationServices.StoreTypeCreations;
using Delivery.Store.Domain.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers
{
    /// <summary>
    ///  Store type apis
    /// </summary>
    [Route("api/v1/[controller]", Name = "4 - Store type apis")]
    [ApiController]
    [Authorize(Policy = "CustomerApiUser")]
    [PlatformSwaggerCategory(ApiCategory.Management)]
    public class StoreTypeController : Controller
    {
        private readonly IServiceProvider serviceProvider;

        public StoreTypeController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///  Create store type
        /// </summary>
        /// <returns></returns>
        [Route("Create-StoreType")]
        [HttpPost]
        [ProducesResponseType(typeof(StoreTypeCreationStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateStoreTypeAsync(StoreTypeCreationContract storeTypeCreationContract)
        {
            var validationResult =
                await new StoreTypeCreationValidator().ValidateAsync(storeTypeCreationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var storeTypeCreationStatusContract = new StoreTypeCreationStatusContract
            {
                StoreTypeId = executingRequestContextAdapter.GetShard().GenerateExternalId(),
                InsertionDateTime = DateTimeOffset.UtcNow
            };

            var storeTypeCreationServiceRequest =
                new StoreTypeCreationServiceRequest(storeTypeCreationContract, storeTypeCreationStatusContract);

            await new StoreTypeCreationService(serviceProvider, executingRequestContextAdapter)
                .ExecuteStoreTypeCreationWorkflowAsync(storeTypeCreationServiceRequest);
            
            return Ok(storeTypeCreationStatusContract);
        }
        
        /// <summary>
        ///  Get store type
        /// </summary>
        /// <returns></returns>
        [Route("GetAllStoreTypes")]
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<StoreTypeContract>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetStoreTypesAsync(CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var storeTypeGetAllQuery =
                new StoreTypeGetAllQuery($"Database-{executingRequestContextAdapter.GetShard().Key}-default-store-types");
            var storeTypeContractList =
                await new StoreTypeGetAllQueryHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(storeTypeGetAllQuery);
          
            return Ok(storeTypeContractList);
        }
    }
}