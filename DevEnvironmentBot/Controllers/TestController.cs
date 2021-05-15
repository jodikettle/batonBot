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


        [HttpGet("{baton}/{prNumber}")]
        public async Task<PullRequest> Get(string baton, int prNumber)
        {
            var repo = getRepoName(baton);
            var test = this.service.GetPRInfo(repo, prNumber);
            return test;
        }

        private string getRepoName(string type)
        {
            if (type == "be")
            {
                return "maraschino";
            }

            if (type == "fe")
            {
                return "ADA-Research-UI";
            }

            if (type == "man")
            {
                return "ADA-Research-Configuration";
            }

            return null;
        }


        [HttpGet("Test")]
        public string GetConfig()
        {
            return DateTime.Now.ToString();
        }
    }
}
