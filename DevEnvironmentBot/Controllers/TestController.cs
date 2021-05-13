using Microsoft.AspNetCore.Mvc;
namespace DevEnvironmentBot.Controllers
{
    using System;
    using System.Threading.Tasks;
    using SharedBaton.GitHubService;
    using SharedBaton.Models;

    [Route("api/[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        private IGitHubService service;

        public TestController(IGitHubService service)
        {
            this.service = service;
        }


        [HttpGet("{prNumber}")]
        public async Task<PullRequest> Get(int prNumber)
        {
            var test =  await this.service.getPRInfo("ADA-Research-UI", prNumber);
            return test;
        }

        [HttpGet("Test")]
        public string GetConfig()
        {
            return DateTime.Now.ToString();
        }
    }
}
