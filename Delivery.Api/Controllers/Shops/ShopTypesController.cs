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

namespace Delivery.Api.Controllers.Shops
{
    /// <summary>
    ///  Shop types controller
    /// </summary>
    [Route("api/v1/shop-owner/shop-types", Name = "1 - Shop Types")]
    [PlatformSwaggerCategory(ApiCategory.ShopOwner)]
    [ApiController]
    public class ShopTypesController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        public ShopTypesController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///  Get store type
        /// </summary>
        /// <param name="cancellationToken"></param>
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