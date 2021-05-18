using Microsoft.AspNetCore.Mvc;
namespace BatonBot.Controllers
{
    using System;
    using BatonBot.GitHubService;
    using BatonBot.Models;
    using BatonBot.Services.RepositoryMapper;

    [Route("api/[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        private readonly IGitHubService service;
        private readonly IRepositoryMapper mapper;

        public TestController(IGitHubService service, IRepositoryMapper mapper)
        {
            this.service = service;
            this.mapper = mapper;
        }


        [HttpGet("{baton}/{prNumber}")]
        public PullRequest Get(string baton, int prNumber)
        {
            var repo = this.mapper.GetRepositoryNameFromBatonName(baton);
            var test = this.service.GetPRInfo(repo, prNumber);
            return test;
        }

        [HttpGet("Test")]
        public string GetConfig()
        {
            return DateTime.Now.ToString();
        }
    }
}
