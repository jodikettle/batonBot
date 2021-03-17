using Microsoft.AspNetCore.Mvc;
namespace DevEnvironmentBot.Controllers
{
    using Microsoft.Extensions.Configuration;

    [Route("api/[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        private string _appId;

        public TestController(IConfiguration configuration)
        {
            _appId = configuration["MicrosoftAppId"];
        }

        [HttpGet]
        public string Get()
        {
            return _appId;
        }
    }
}
