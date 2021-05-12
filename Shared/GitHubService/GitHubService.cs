namespace SharedBaton.GitHubService
{
    using System.Threading.Tasks;
    using Flurl;
    using Flurl.Http;
    using Microsoft.Extensions.Configuration;

    public class GitHubService : IGitHubService
    {
        private readonly string gitHubApiUrl;

        public GitHubService(IConfiguration config)
        {
            this.gitHubApiUrl = config["GithubUrl"];
        }

        public Task CloseTicket(string repo, int prNumber)
        {
            return null;
        }

        public async Task<bool> UpdatePullRequest(string repo, int prNumber)
        {
            var result = await this.gitHubApiUrl
                .AppendPathSegment($"/repos/redington/{repo}/pulls/{prNumber}/update-branch")
                .WithHeader("Accept", "application/vnd.github.lydian-preview+json")
                .PutAsync();

            return result.StatusCode == 202;
        }
    }
}
