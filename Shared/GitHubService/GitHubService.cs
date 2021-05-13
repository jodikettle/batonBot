namespace SharedBaton.GitHubService
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Flurl;
    using Flurl.Http;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using RestSharp;
    using SharedBaton.Models;

    public class GitHubService : IGitHubService
    {
        private readonly string gitHubApiUrl;
        private readonly string gitHubAccessToken;

        public GitHubService(IConfiguration config)
        {
            this.gitHubApiUrl = config["GithubUrl"];
            this.gitHubAccessToken = config["GitHubApiKey"];
        }

        public async Task<bool> CloseTicket(string repo, int issueNumber)
        {
            var result = await this.gitHubApiUrl
                .AppendPathSegment($"/repos/ada/{repo}/issues/{issueNumber}")
                .WithHeaders(new
                {
                    Accept = "application/vnd.github.v3+json",
                    Authorization = $"token {gitHubAccessToken}"
                })
                .PatchAsync();

            return result.StatusCode == 200;
        }

        public async Task<PullRequest> getPRInfo(string repo, int prNumber)
        {
            // GET / repos /{ owner}/{ repo}/ pulls /{ pull_number}
            var client = new RestClient($"https://api.github.com/repos/redington/{repo}/pulls/{prNumber}");
            var request = new RestRequest(Method.GET);
            request.AddHeader("postman-token", "4783d919-ab08-8f57-f2d0-a63200c68ee4");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("authorization", $"token {gitHubAccessToken}");
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<PullRequest>(response.Content);
            }

            //var result = await this.gitHubApiUrl
            //    .AppendPathSegment($"/repos/redington/{repo}/pulls/{prNumber}")
            //    .WithHeaders(new
            //    {
            //        Accept = "application/vnd.github.v3+json",
            //        Authorization = "token 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"
            //    })
            //    .GetJsonAsync<PullRequest>();


            return null;
        }
        public async Task<bool> MergePullRequest(string repo, int prNumber)
        {
            //Get Pull Request 
            var pr = await this.getPRInfo(repo, prNumber);

            if (pr.mergeable_state != "blocked" && pr.merged)
            {

                var result = await this.gitHubApiUrl
                    .AppendPathSegment($"/repos/redington/{repo}/pulls/{prNumber}/merge")
                    .WithHeaders(new
                    {
                        Accept = "application/vnd.github.v3+json",
                        Authorization = $"token {gitHubAccessToken}"
                    })
                    .PutJsonAsync(
                        new
                        {
                            commit_title = pr.head.reference,
                            commit_message = pr.GetMergeDecriptionString(),
                            merge_method = "squash"
                        });

                return result.StatusCode == 200;
            }
            else
            {
                return false;
            }
        }

        public async Task<ServiceResult> UpdatePullRequest(string repo, int prNumber)
        {
            var pr = await this.getPRInfo(repo, prNumber);

            if (pr.mergeable_state == "behind")
            {
                var result = await this.gitHubApiUrl
                    .AppendPathSegment($"/repos/redington/{repo}/pulls/{prNumber}/update-branch")
                    .WithHeaders(
                        new
                        {
                            Accept = "application/vnd.github.lydian-preview+json",
                            Authorization = $"token {gitHubAccessToken}"
                        })
                    .PutAsync();

                return result.StatusCode == 202
                    ? new ServiceResult()
                    {
                        Succeeded = true
                    }
                    : new ServiceResult()
                    {
                        Succeeded = false,
                        ReasonForFailure = result.ResponseMessage.ToString()
                    };
            }
            return new ServiceResult()
            {
                Succeeded = false,
                ReasonForFailure = "Not Needed"
            };
        }
    }
}
