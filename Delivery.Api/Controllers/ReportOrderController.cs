using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Api.CommandHandler;
using Delivery.Api.Domain.Command;
using Delivery.Api.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static Delivery.Api.Extensions.HttpResults;


namespace Delivery.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportOrderController : ControllerBase
    {

        private readonly ILogger<ReportOrderController> _logger;
        private readonly ICommandHandler<CreateReportOrderCommand, bool> _createReportOrderCommand;
        private readonly IMapper _mapper;

        public ReportOrderController(
            ILogger<ReportOrderController> logger,
            ICommandHandler<CreateReportOrderCommand, bool> createReportOrderCommand,
            IMapper mapper)
        {
            _logger = logger;
            _createReportOrderCommand = createReportOrderCommand;
        }

        // POST api/values
        [HttpPost("Create")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddReport(ReportDto reportDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createReportOrderCommand = _mapper.Map<CreateReportOrderCommand>(reportDto);

                await _createReportOrderCommand.Handle(createReportOrderCommand);

                return Ok();
            }
            catch(Exception ex)
            {
                var errorMessage = "Error occurred in creating order";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }

        }
    }
}
