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
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Customer.Domain.CommandHandlers;
using Delivery.Customer.Domain.Contracts.RestContracts;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.FrameWork.Context;
using Delivery.User.Domain.Contracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Delivery.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        private readonly SignInManager<Database.Models.ApplicationUser> _signInManager;
        private readonly UserManager<Database.Models.ApplicationUser> _userManager;

        private readonly ApplicationDbContext _appDbContext;
        private readonly IServiceProvider serviceProvider;

        public UserController(ILogger<UserController> logger,
             UserManager<Database.Models.ApplicationUser> userManager,
             SignInManager<Database.Models.ApplicationUser> signInManager,
             ApplicationDbContext appDbContext,
             IServiceProvider serviceProvider
             )
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _appDbContext = appDbContext;
            this.serviceProvider = serviceProvider;
        }

        // POST: api/User
        [HttpPost("register")]
        public async Task<IActionResult> Post([FromBody] RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            _appDbContext.SetExecutingRequestContextAdapter(serviceProvider,executingRequestContextAdapter);

            var user = new Database.Models.ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var customerCreationContract = new CustomerCreationContract();
                customerCreationContract.IdentityId = user.Id;
                customerCreationContract.Username = user.Email;
                
                
                var createCustomerCommand = new CreateCustomerCommand(customerCreationContract);
                var createCustomerCommandHandler =
                    new CreateCustomerCommandHandler(serviceProvider, executingRequestContextAdapter); 
                await createCustomerCommandHandler.Handle(createCustomerCommand);
                
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
