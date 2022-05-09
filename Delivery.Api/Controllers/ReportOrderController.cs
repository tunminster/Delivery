using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Api.Models.Dto;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.FrameWork.Context;
using Delivery.Order.Domain.Contracts.V1.RestContracts;
using Delivery.Order.Domain.Handlers.CommandHandlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers
{
    /// <summary>
    ///  Report order apis
    /// </summary>
    [Route("api/[controller]" , Name = "6 - Report order")]
    [ApiController]
    [Authorize]
    [PlatformSwaggerCategory(ApiCategory.Customer)]
    public class ReportOrderController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        public ReportOrderController(
            IServiceProvider serviceProvider
            )
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        ///  Add report
        /// </summary>
        /// <param name="reportCreationContract"></param>
        /// <returns></returns>
        [Route("Create", Order = 1)]
        [HttpPost]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddReportAsync(ReportCreationContract reportCreationContract)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var createReportOrderCommand = new CreateReportOrderCommand(reportCreationContract);
            var reportOrderCommandHandler =
                new ReportOrderCommandHandler(serviceProvider, executingRequestContextAdapter);

            await reportOrderCommandHandler.HandleAsync(createReportOrderCommand);

            return Ok();
        }
    }
}
