using Microsoft.AspNetCore.Mvc;
namespace DevEnvironmentBot.Controllers
{
    using Microsoft.Extensions.Configuration;

    public class TestController : Controller
    {
        private string _appId;

        public TestController(IConfiguration configuration)
        {
            _appId = configuration["MicrosoftAppId"];
        }

        public string Index()
        {
            return _appId;
        }
    }
}
