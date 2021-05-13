namespace SharedBaton.GitHubService
{
    using System;
    using System.Net;
    using System.Text.RegularExpressions;
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

        public async Task<bool> CloseTicket(string repo, int prNumber)
        {
            if (string.IsNullOrEmpty(repo))
            {
                return false;
            }
            if (prNumber < 1)
            {
                return false;
            }

            var pr = await this.getPRInfo(repo, prNumber);

            var resultString = Regex.Match(pr.head.reference, @"\d+").Value;
            int.TryParse(resultString, out int issueResult);

            if (issueResult <= 0) return false;

            try
            {
                var result = await this.gitHubApiUrl
                    .AppendPathSegment($"/repos/redington/ada/issues/{issueResult}")
                    .WithHeaders(
                        new
                        {
                            Accept = "application/vnd.github.v3+json",
                            Authorization = $"token {this.gitHubAccessToken}",
                            User_Agent = "BatonBot"
                        })
                    .PatchJsonAsync(
                        new
                        {
                            state = "closed"
                        });

                return result.StatusCode == 200;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<PullRequest> getPRInfo(string repo, int prNumber)
        {
            // GET / repos /{ owner}/{ repo}/ pulls /{ pull_number}
            var client = new RestClient($"https://api.github.com/repos/redington/{repo}/pulls/{prNumber}");
            var request = new RestRequest(Method.GET);
            request.AddHeader("authorization", $"token {gitHubAccessToken}");
            IRestResponse response = client.Execute(request);

            return response.StatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<PullRequest>(response.Content) : null;
        }
        public async Task<ServiceResult> MergePullRequest(string repo, int prNumber)
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
                            commit_title = pr.title,
                            commit_message = pr.GetMergeDecriptionString(),
                            merge_method = "squash"
                        });

                return result.StatusCode == 200
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
                ReasonForFailure = "No clue"
            };
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
