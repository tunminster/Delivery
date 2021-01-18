using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.FrameWork.Context;
using Delivery.Product.Domain.QueryHandlers;
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
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiUser")]
    public class StoreTypeController : Controller
    {
        private readonly IServiceProvider serviceProvider;

        public StoreTypeController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        
        [HttpPost("Create-StoreType")]
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
        
        [HttpGet("GetAllStoreTypes")]
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