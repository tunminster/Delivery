using System;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Api.Models.Dto;
using Delivery.Domain.CommandHandlers;
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
        private readonly ICommandHandler<CreateReportOrderCommand, bool> _createReportOrderCommand;
        private readonly IMapper _mapper;

        public ReportOrderController(
            ICommandHandler<CreateReportOrderCommand, bool> createReportOrderCommand,
            IMapper mapper)
        {
            _createReportOrderCommand = createReportOrderCommand;
            _mapper = mapper;
        }

        // POST api/values
        [HttpPost("Create")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddReport(ReportCreationContract reportCreationContract)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var createReportOrderCommand = new CreateReportOrderCommand(reportCreationContract);

            await _createReportOrderCommand.Handle(createReportOrderCommand);

            return Ok();
        }
    }
}
