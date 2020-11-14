using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Delivery.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Delivery.Api.Models.Dto;
using Delivery.Customer.Domain.CommandHandlers;
using Delivery.Customer.Domain.Contracts.RestContracts;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Domain.CommandHandlers;
using Delivery.User.Domain.Contracts;

namespace Delivery.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _appDbContext;
        private readonly ICommandHandler<CreateCustomerCommand, bool> customerCreationCommandHandler;

        public UserController(ILogger<UserController> logger,
             UserManager<ApplicationUser> userManager,
             SignInManager<ApplicationUser> signInManager,
             IEmailSender emailSender,
             ApplicationDbContext appDbContext,
             ICommandHandler<CreateCustomerCommand, bool> customerCreationCommandHandler)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _appDbContext = appDbContext;
            this.customerCreationCommandHandler = customerCreationCommandHandler;
        }

        // POST: api/User
        [HttpPost("register")]
        public async Task<IActionResult> Post([FromBody] RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var customerCreationContract = new CustomerCreationContract();
                customerCreationContract.IdentityId = user.Id;
                customerCreationContract.Username = user.Email;
                
                var createCustomerCommand = new CreateCustomerCommand(customerCreationContract);
                await customerCreationCommandHandler.Handle(createCustomerCommand);
                
                return new OkObjectResult("Account created");
            }
            return new BadRequestObjectResult(Errors.AddErrorsToModelState(result, ModelState));

        }

        [HttpGet("GetUser")]
        [Authorize]
        [ProducesResponseType(typeof(UserContract), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult GetUser()
        {
            string userName = HttpContext?.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var result = new UserContract { UserName = userName };
            return Ok(result);
        }

        
    }
}
