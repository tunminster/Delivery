using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.FrameWork.Context;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreGeoUpdate;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreUpdate;
using Delivery.Store.Domain.ElasticSearch.Handlers.QueryHandlers.StoreSearchQueries;
using Delivery.Store.Domain.Handlers.CommandHandlers.StoreCreation;
using Delivery.Store.Domain.Handlers.CommandHandlers.StoreGeoUpdate;
using Delivery.Store.Domain.Handlers.QueryHandlers.StoreDetailsQueries;
using Delivery.Store.Domain.Handlers.QueryHandlers.StoreGetQueries;
using Delivery.Store.Domain.Services.ApplicationServices.StoreCreations;
using Delivery.Store.Domain.Services.ApplicationServices.StoreUpdates;
using Delivery.Store.Domain.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Delivery.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiUser")]
    public class StoreController : Controller
    {
        private readonly IServiceProvider serviceProvider;

        public StoreController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///  Store: Create store endpoint allows to create store
        /// </summary>
        /// <param name="storeCreationContract"></param>
        /// <returns></returns>
        [HttpPost("CreateStore")]
        [ProducesResponseType(typeof(StoreCreationStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateStoreAsync(StoreCreationContract storeCreationContract)
        {
            var validationResult =
                await new StoreCreationValidator().ValidateAsync(storeCreationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var storeCreationStatusContract = new StoreCreationStatusContract
            {
                StoreId = executingRequestContextAdapter.GetShard().GenerateExternalId(),
                InsertionDateTime = DateTimeOffset.UtcNow
            };

            var storeCreationServiceRequest =
                new StoreCreationServiceRequest(storeCreationContract, storeCreationStatusContract);

            await new StoreCreationService(serviceProvider, executingRequestContextAdapter)
                .ExecuteStoreCreationWorkflowAsync(storeCreationServiceRequest);
            
            return Ok(storeCreationStatusContract);
        }
        
        /// <summary>
        ///  Store: Update store endpoint allows to update store
        /// </summary>
        /// <param name="storeUpdateContract"></param>
        /// <returns></returns>
        [HttpPost("UpdateStore")]
        [ProducesResponseType(typeof(StoreCreationStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateStoreAsync(StoreUpdateContract storeUpdateContract)
        {
            var validationResult =
                await new StoreUpdateValidator().ValidateAsync(storeUpdateContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var storeUpdateStatusContract = new StoreUpdateStatusContract
            {
                StoreId = executingRequestContextAdapter.GetShard().GenerateExternalId(),
                InsertionDateTime = DateTimeOffset.UtcNow
            };

            var storeUpdateServiceRequest =
                new StoreUpdateServiceRequest(storeUpdateContract, storeUpdateStatusContract);
            
            await new StoreUpdateService(serviceProvider, executingRequestContextAdapter)
                .ExecuteStoreUpdateWorkflowAsync(storeUpdateServiceRequest);
            
            return Ok(storeUpdateStatusContract);
        }
        
        // [HttpGet("GetAllStores")]
        // [ProducesResponseType(typeof(List<StoreContract>), (int)HttpStatusCode.OK)]
        // [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        // public async Task<IActionResult> GetStoresAsync(string numberOfObjectPerPage, string pageNumber, CancellationToken cancellationToken = default)
        // {
        //     var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
        //
        //     int.TryParse(numberOfObjectPerPage, out var iNumberOfObjectPerPage);
        //     int.TryParse(pageNumber, out var iPageNumber);
        //
        //     var storeGetAllQuery =
        //         new StoreGetAllQuery($"Database-{executingRequestContextAdapter.GetShard().Key}-store-{iNumberOfObjectPerPage}-{iPageNumber}", iNumberOfObjectPerPage, iPageNumber);
        //     var storeContractList =
        //         await new StoreGetAllQueryHandler(serviceProvider, executingRequestContextAdapter)
        //             .Handle(storeGetAllQuery);
        //
        //     await IndexStoreAsync(storeContractList.FirstOrDefault());
        //
        //     var result = GetStoreAsync("thai", 1, 5);
        //   
        //     return Ok(storeContractList);
        // }

        /// <summary>
        ///  Store: Search store endpoint allows to search stores 
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <param name="filters"></param>
        /// <param name="storeTypes"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("Stores-Search")]
        [ProducesResponseType(typeof(List<StoreContract>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetStoreSearchAsync(string searchQuery, string filters, string storeTypes,
            string latitude, string longitude, string page, string pageSize, CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            int.TryParse(pageSize, out var iPageSize);
            int.TryParse(page, out var iPage);
            double.TryParse(latitude, out var dLatitude);
            double.TryParse(longitude, out var dLongitude);
            
            var defaultDistance = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSetting<string>("DefaultDistance");

            var storeSearchQuery = new StoreSearchQuery(searchQuery, iPage, iPageSize, storeTypes, dLatitude,
                dLongitude, defaultDistance);

            var storeContractList =
                await new StoreSearchQueryHandler(serviceProvider, executingRequestContextAdapter).Handle(storeSearchQuery);

            return Ok(storeContractList);

        }

        /// <summary>
        ///  Store: Get store details by store id.
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [HttpGet("Store-Details")]
        [ProducesResponseType(typeof(List<StoreContract>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetStoreDetailsByIdAsync(string storeId)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var storeDetailsGetByIdQuery =
                new StoreDetailsGetByIdQuery(storeId, $"Database-{executingRequestContextAdapter.GetShard().Key}-default-store-details-{storeId}");

            var storeDetailsList =
                await new StoreDetailsGetByIdQueryHandler(serviceProvider, executingRequestContextAdapter).Handle(
                    storeDetailsGetByIdQuery);

            return Ok(storeDetailsList);
        }
        
        // [HttpGet("Get-Nearest-Stores")]
        // [ProducesResponseType(typeof(List<StoreContract>), (int)HttpStatusCode.OK)]
        // [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        // public async Task<IActionResult> GetNearestStoresAsync(string numberOfObjectPerPage, string pageNumber, string latitude, string longitude, CancellationToken cancellationToken = default)
        // {
        //     var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
        //
        //     int.TryParse(numberOfObjectPerPage, out var iNumberOfObjectPerPage);
        //     int.TryParse(pageNumber, out var iPageNumber);
        //     double.TryParse(latitude, out var dLatitude);
        //     double.TryParse(longitude, out var dLongitude);
        //     
        //     var storeGetByNearestLocationQuery =
        //         new StoreGetByNearestLocationQuery($"Database-{executingRequestContextAdapter.GetShard().Key}-store-{iNumberOfObjectPerPage}-{iPageNumber}-{latitude}-{longitude}", iNumberOfObjectPerPage, iPageNumber, dLatitude, dLongitude, 7);
        //     
        //     var storeContractList =
        //         await new StoreGetByStoreTypeIdAndGeoLocationQuery(serviceProvider, executingRequestContextAdapter)
        //             .Handle(storeGetByNearestLocationQuery);
        //   
        //     return Ok(storeContractList);
        // }

        private async Task IndexStoreAsync(StoreContract storeContract)
        {
            var elasticClient = serviceProvider.GetRequiredService<IElasticClient>();
            
            var createIndexResponse = await elasticClient.Indices.CreateAsync("stores", c => c
                .Map<StoreContract>(m => m.AutoMap()
                    .Properties(p => p
                        .GeoPoint(d => d
                            .Name(n =>n.Location)
                        )
                    )
                )
            );
            
            var createResponse = await elasticClient.CreateAsync(storeContract,
                i => i
                    .Index("stores")
                    .Id(storeContract.StoreId)
            );
            
            
            
            
            // var deleteResponse = await elasticClient.DeleteAsync<StoreContract>(storeContract.StoreId, d => d
            //     .Index("stores")
            // );

            //var deleteIndexResponse = await elasticClient.Index("sto
            
            
            
        }

        private async Task<List<StoreContract>> GetStoreAsync(string query, int page = 1, int pageSize = 5)
        {
            var elasticClient = serviceProvider.GetRequiredService<IElasticClient>();
            
            var searchResponse = elasticClient.Search<StoreContract>(s => s
                .AllIndices()
                .QueryOnQueryString("storename:thai")
                
            );
            // var response = await elasticClient.SearchAsync<StoreContract>(
            //     x => x.Query(q =>
            //             q.QueryString(d => d.Query(query)))
            //         .From((page - 1) * pageSize)
            //         .Size(pageSize)
            // );

            var storeContracts = searchResponse.Documents.ToList();

            return storeContracts;
        }
        
    }
}