using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Domain.FrameWork.Context;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;
using Delivery.Store.Domain.Handlers.QueryHandlers.StoreTypeGetQueries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Customers
{
    /// <summary>
    ///  Customer controller
    /// </summary>
    [Route("api/customerStoreType", Name = "13 - Customer Store Type")]
    [PlatformSwaggerCategory(ApiCategory.Customer)]
    [ApiController]
    [Authorize]
    public class CustomerStoreTypeController : Controller
    {
        private readonly IServiceProvider serviceProvider;

        public CustomerStoreTypeController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
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