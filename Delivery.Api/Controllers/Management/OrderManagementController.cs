using System;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Database.Constants;
using Delivery.Domain.FrameWork.Context;
using Delivery.Order.Domain.Contracts.RestContracts;
using Delivery.Order.Domain.Handlers.QueryHandlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Management
{
    /// <summary>
    ///  Order management
    /// </summary>
    [Route("api/v1/management/orders" , Name = "8 - Order management")]
    [PlatformSwaggerCategory(ApiCategory.Management)]
    [ApiController]
    [Authorize(Roles = RoleConstant.Administrator)]
    public class OrderManagementController : Controller
    {
        private readonly IServiceProvider serviceProvider;
        
        public OrderManagementController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///  Get order list
        /// </summary>
        /// <returns></returns>
        [Route("get-orders", Order = 1)]
        [HttpGet]
        [Authorize(Roles = RoleConstant.ShopOwner)]
        [ProducesResponseType(typeof(OrderPagedContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get_OrdersAsync(string pageSize, string pageNumber)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            int.TryParse(pageSize, out var iPageSize);
            int.TryParse(pageNumber, out var iPageNumber);
        
            var orderGetAllQuery =
                new OrderGetAllQuery(string.Empty, iPageSize, iPageNumber);
            var orderPagedContract =
                await new OrderGetAllQueryHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(orderGetAllQuery);
            
            return Ok(orderPagedContract);
        }
    }
}