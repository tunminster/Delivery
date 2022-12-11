using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Domain.FrameWork.Context;
using Delivery.Driver.Domain.Constants;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverApproval;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverSearch;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverApproval;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverTimerRejection;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverAssignment;
using Delivery.Driver.Domain.Handlers.QueryHandlers.DriverAssignment;
using Delivery.Driver.Domain.Handlers.QueryHandlers.DriverProfile;
using Delivery.Driver.Domain.Validators;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopOrderManagement;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders;
using Delivery.Shop.Domain.Handlers.MessageHandlers.ShopOrderManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Management
{
    /// <summary>
    ///  Driver management
    /// </summary>
    [Route("api/v1/management/driver-management" , Name = "2 - Driver management")]
    [PlatformSwaggerCategory(ApiCategory.Management)]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public class DriverManagementController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        
        public DriverManagementController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        ///  Driver list
        /// </summary>
        /// <returns></returns>
        [Route("drivers", Order = 1)]
        [ProducesResponseType(typeof(DriversPageContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpGet]
        public async Task<IActionResult> Get_DriversAsync(string pageNumber, string pageSize)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            if (string.IsNullOrEmpty(pageNumber) || string.IsNullOrEmpty(pageSize))
            {
                var errorMessage = $"{nameof(pageNumber)} and {nameof(pageSize)} are required";

                return errorMessage.ConvertToBadRequest();
            }

            int.TryParse(pageNumber, out var iPageNumber);
            int.TryParse(pageSize, out var iPageSize);


            var driverQuery = new DriversQuery(iPageNumber, iPageSize);

            var driversPageContract = await new DriversQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(driverQuery);

            return Ok(driversPageContract);
        }
        
        /// <summary>
        ///  Driver approval
        /// </summary>
        /// <returns></returns>
        [Route("approve-driver", Order = 2)]
        [ProducesResponseType(typeof(DriverApprovalStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_ApproveAsync(DriverApprovalContract driverApprovalContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var driverApprovalCommand = new DriverApprovalCommand(driverApprovalContract);

            var driverApprovalStatusContract = await new DriverApprovalCommandHandler(serviceProvider, executingRequestContextAdapter).HandleAsync(
                driverApprovalCommand);
            
            return Ok(driverApprovalStatusContract);
        }

        /// <summary>
        /// Run driver status
        /// </summary>
        /// <returns></returns>
        [Route("run-driver-status", Order = 3)]
        [ProducesResponseType(typeof(StatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Run_StatusAsync()
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var driverTimerRejectionCommand =
                new DriverTimerRejectionCommand(executingRequestContextAdapter.GetShard().Key);

            var statusContract =
                await new DriverTimerRejectionCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .HandleAsync(driverTimerRejectionCommand);

            return Ok(statusContract);
        }
        
        /// <summary>
        ///  Request delivery driver for the order.
        /// </summary>
        /// <returns></returns>
        [Route("request-delivery-driver", Order = 4)]
        [HttpPost]
        [ProducesResponseType(typeof(ShopOrderContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delivery_DriverRequest_Async(ShopOrderDriverRequestContract shopOrderDriverRequestContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            if (string.IsNullOrEmpty(shopOrderDriverRequestContract.OrderId))
            {
                var errorMessage = $"{nameof(shopOrderDriverRequestContract.OrderId)} must be provided.";
                return errorMessage.ConvertToBadRequest();
            }
            
            var statusContract = new StatusContract
            {
                Status = true,
                DateCreated = DateTimeOffset.UtcNow
            };
            
            var shopOrderDriverRequestMessageContract = new ShopOrderDriverRequestMessageContract
            {
                PayloadIn = shopOrderDriverRequestContract,
                PayloadOut = statusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };
            
            await new ShopOrderDriverRequestMessagePublisher(serviceProvider).PublishAsync(shopOrderDriverRequestMessageContract);

            return Ok(statusContract);
        }
        
        /// <summary>
        ///  Search driver
        /// </summary>
        /// <returns></returns>
        [Route("search-driver")]
        [HttpPost]
        [ProducesResponseType(typeof(List<DriverContract>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get_Nearest_Drivers_Async(DriverSearchCreationContract driverSearchCreationContract, CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var driverByNearestLocationQuery = new DriverByNearestLocationQuery
            {
                Latitude = driverSearchCreationContract.Latitude,
                Longitude = driverSearchCreationContract.Longitude,
                Distance = driverSearchCreationContract.Distance,
                Page = driverSearchCreationContract.Page,
                PageSize = driverSearchCreationContract.PageSize
            };

            var driverContracts =
                await new DriverByNearestLocationQueryHandler(serviceProvider, executingRequestContextAdapter).Handle(
                    driverByNearestLocationQuery);

            return Ok(driverContracts);
        }
        
        /// <summary>
        ///  Search drivers by freetext
        /// </summary>
        /// <returns></returns>
        [Route("search-driver-name")]
        [HttpPost]
        [ProducesResponseType(typeof(List<DriverContract>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Search_Drivers_by_Name_Async(
            DriverSearchByNameContract driverSearchByNameContract, CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var driverSearchQuery = new DriverByNameQuery(driverSearchByNameContract.FreeTextSearch,
                driverSearchByNameContract.Page, driverSearchByNameContract.PageSize);

            var drivers = await new DriverByNameQueryHandler(serviceProvider, executingRequestContextAdapter)
                .HandleAsync(driverSearchQuery);

            return Ok(drivers);
        }
        
    }
}