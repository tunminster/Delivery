using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Azure.Library.WebApi.ModelBinders;
using Delivery.Domain.FrameWork.Context;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreImageCreations;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreUpdate;
using Delivery.Store.Domain.ElasticSearch.Contracts.V1.RestContracts.StoreIndexing;
using Delivery.Store.Domain.ElasticSearch.Handlers.CommandHandlers.StoreIndexing;
using Delivery.Store.Domain.ElasticSearch.Handlers.QueryHandlers.StoreSearchQueries;
using Delivery.Store.Domain.ElasticSearch.Validators;
using Delivery.Store.Domain.Handlers.CommandHandlers.StoreImageCreation;
using Delivery.Store.Domain.Handlers.QueryHandlers.StoreDetailsQueries;
using Delivery.Store.Domain.Handlers.QueryHandlers.StoreGetQueries;
using Delivery.Store.Domain.Services.ApplicationServices.StoreCreations;
using Delivery.Store.Domain.Services.ApplicationServices.StoreUpdates;
using Delivery.Store.Domain.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Delivery.Api.Controllers
{
    /// <summary>
    ///  Store controller
    /// </summary>
    [Route("api/v1/[controller]", Name = "7 - Store apis")]
    [ApiController]
    [Authorize(Policy = "CustomerApiUser")]
    [PlatformSwaggerCategory(ApiCategory.Customer)]
    public class StoreController : Controller
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        ///  store controller contructor
        /// </summary>
        /// <param name="serviceProvider"></param>
        public StoreController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        ///  Store: Create store endpoint allows to create store
        /// </summary>
        /// <param name="storeCreationContract"></param>
        /// <param name="storeImage"></param>
        /// <returns></returns>
        [Route("CreateStore", Order = 1)]
        [HttpPost]
        [ProducesResponseType(typeof(StoreCreationStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateStoreAsync([ModelBinder(BinderType = typeof(JsonModelBinder))] StoreCreationContract storeCreationContract, IFormFile? storeImage)
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

            // upload image
            if (storeImage != null)
            {
                var storeImageCreationStatusContract = await
                    UploadStoreImageAsync(storeCreationStatusContract.StoreId, storeCreationContract.StoreName, storeImage,
                        executingRequestContextAdapter);

                storeCreationContract.ImageUri = storeImageCreationStatusContract.ImageUri;
            }
            
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
        /// <param name="storeImage"></param>
        /// <returns></returns>
        [Route("UpdateStore", Order = 2)]
        [HttpPut]
        [ProducesResponseType(typeof(StoreCreationStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateStoreAsync([ModelBinder(BinderType = typeof(JsonModelBinder))] StoreUpdateContract storeUpdateContract, IFormFile storeImage)
        {
            var validationResult =
                await new StoreUpdateValidator().ValidateAsync(storeUpdateContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            // upload image
            if (storeImage != null)
            {
                var storeImageUpdateStatusContract = await
                    UploadStoreImageAsync(storeUpdateContract.StoreId, storeUpdateContract.StoreName, storeImage,
                        executingRequestContextAdapter);

                storeUpdateContract.ImageUri = storeImageUpdateStatusContract.ImageUri;
            }

            var storeUpdateStatusContract = new StoreUpdateStatusContract
            {
                StoreId = storeUpdateContract.StoreId,
                InsertionDateTime = DateTimeOffset.UtcNow
            };

            var storeUpdateServiceRequest =
                new StoreUpdateServiceRequest(storeUpdateContract, storeUpdateStatusContract);
            
            await new StoreUpdateService(serviceProvider, executingRequestContextAdapter)
                .ExecuteStoreUpdateWorkflowAsync(storeUpdateServiceRequest);
            
            return Ok(storeUpdateStatusContract);
        }

        /// <summary>
        ///  Store: indexing 
        /// </summary>
        /// <param name="storeIndexCreationContract"></param>
        /// <returns></returns>
        [Route("Index-Store", Order = 3)]
        [HttpPost]
        [ProducesResponseType(typeof(StoreIndexCreationContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> StoreIndexAsync(StoreIndexCreationContract storeIndexCreationContract)
        {
            var validationResult =
                await new StoreIndexCreationValidator().ValidateAsync(storeIndexCreationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var storeIndexCommand = new StoreIndexCommand(storeIndexCreationContract, new StoreIndexStatusContract());
            var storeIndexStatusContract =
                await new StoreIndexCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(storeIndexCommand);

            return Ok(storeIndexStatusContract);
        }
        
        /// <summary>
        ///  Get all stores
        /// </summary>
        /// <param name="numberOfObjectPerPage"></param>
        /// <param name="pageNumber"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Route("GetAllStores", Order = 4)]
        [HttpGet]
        [ProducesResponseType(typeof(List<StoreContract>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetStoresAsync(string numberOfObjectPerPage, string pageNumber, CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
        
            int.TryParse(numberOfObjectPerPage, out var iNumberOfObjectPerPage);
            int.TryParse(pageNumber, out var iPageNumber);
        
            var storeGetAllQuery =
                new StoreGetAllQuery(iNumberOfObjectPerPage, iPageNumber);
            var storePagedContract =
                await new StoreGetAllQueryHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(storeGetAllQuery);
            
            return Ok(storePagedContract.Data);
        }

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
        [Route("Stores-Search", Order = 5)]
        [HttpGet]
        [ProducesResponseType(typeof(List<StoreContract>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetStoreSearchAsync(string? searchQuery, string? filters, string? storeTypes,
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
        ///  Search stores by store type
        /// </summary>
        /// <param name="storeType"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Route("Stores-Search-By-Store-Type", Order = 6)]
        [HttpGet]
        [ProducesResponseType(typeof(List<StoreContract>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetStoreSearchByStoreTypeAsync(string storeType,
            string latitude, string longitude, string page, string pageSize, CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            int.TryParse(pageSize, out var iPageSize);
            int.TryParse(page, out var iPage);
            double.TryParse(latitude, out var dLatitude);
            double.TryParse(longitude, out var dLongitude);
            
            var defaultDistance = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSetting<string>("DefaultDistance");

            var storeSearchQueryByStoreTypeQuery = new StoreSearchQueryByStoreTypeQuery(storeType, iPage, iPageSize, dLatitude,
                dLongitude, defaultDistance);

            var storeContractList =
                await new StoreSearchQueryByStoreTypeQueryHandler(serviceProvider, executingRequestContextAdapter).Handle(storeSearchQueryByStoreTypeQuery);

            return Ok(storeContractList);

        }

        /// <summary>
        ///  Store: Get store details by store id.
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [Route("Store-Details", Order = 7)]
        [HttpGet]
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

        /// <summary>
        ///  Get external id
        /// </summary>
        /// <returns></returns>
        [Route("Store-Generate-Id", Order = 8)]
        [HttpGet]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        public IActionResult GetExternalId()
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            return Ok(executingRequestContextAdapter.GetShard().GenerateExternalId());
        }
        
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

        private async Task<StoreImageCreationStatusContract> UploadStoreImageAsync(string storeId, string storeName, IFormFile storeImage, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            var storeImageCreationContract = new StoreImageCreationContract
            {
                StoreId = storeId,
                StoreImage = storeImage,
                StoreName = storeName
            };

            var storeImageCreationCommand = new StoreImageCreationCommand(storeImageCreationContract);

            var storeImageCreationStatusContract =
                await new StoreImageCreationCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .HandleCoreAsync(storeImageCreationCommand);

            return storeImageCreationStatusContract;
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