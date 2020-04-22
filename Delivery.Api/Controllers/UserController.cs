using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Delivery.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        private readonly IEmailSender _emailSender;


        public UserController(ILogger<UserController> logger,
             UserManager<IdentityUser> userManager,
             SignInManager<IdentityUser> signInManager,
             IEmailSender emailSender)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        // GET: api/User
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/User/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        public async Task<IActionResult> Login([FromBody]InputModel data)
        {
            var result = await _signInManager.PasswordSignInAsync(data.Email, data.Password, true, lockoutOnFailure: false);
            if (result.Succeeded)
            {

            }

            return Ok();
        }

        // POST: api/User
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] InputModel value)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = value.Email, Email = value.Email };
                var result = await _userManager.CreateAsync(user, value.Password);
                
                if (result.Succeeded)
                {
                    return Accepted();
                }
                    
            }
            return BadRequest(ModelState);
        }

        // PUT: api/User/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
