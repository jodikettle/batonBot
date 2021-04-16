using Microsoft.AspNetCore.Mvc;
namespace DevEnvironmentBot.Controllers
{
    using System;
    using Microsoft.Extensions.Configuration;

    [Route("api/[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        private string _appId;

        public TestController(IConfiguration configuration)
        {
            _appId = configuration["WEBSITE_TIME_ZONE"];
        }

        [HttpGet]
        public string Get()
        {
            return _appId;
        }

        [HttpGet("Test")]
        public string GetConfig()
        {
            return DateTime.Now.ToString();
        }
    }
}
