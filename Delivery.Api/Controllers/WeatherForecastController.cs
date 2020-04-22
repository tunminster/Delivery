using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Delivery.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        //private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        
        private readonly IEmailSender _emailSender;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
             UserManager<ApplicationUser> userManager,
            
             IEmailSender emailSender)
        {
            _logger = logger;
            _userManager = userManager;
            //_signInManager = signInManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var rng = new Random();

            var user = new ApplicationUser { UserName = "test", Email = "test123@gmail.com" };
            var result = await _userManager.CreateAsync(user, "Password123");

            if(result.Succeeded)
            {
                var test = "ok";
            }

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
