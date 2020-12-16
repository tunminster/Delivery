using System;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Api.Models.Dto;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.FrameWork.Context;
using Delivery.Order.Domain.CommandHandlers;
using Delivery.Order.Domain.Contracts.RestContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportOrderController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        public ReportOrderController(
            IServiceProvider serviceProvider
            )
        {
            this.serviceProvider = serviceProvider;
        }

        // POST api/values
        [HttpPost("Create")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
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

            await reportOrderCommandHandler.Handle(createReportOrderCommand);

            return Ok();
        }
    }
}
