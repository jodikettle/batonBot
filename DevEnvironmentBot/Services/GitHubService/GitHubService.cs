﻿namespace BatonBot.GitHubService
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
    using BatonBot.Models;

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

            var pr = this.GetPRInfo(repo, prNumber);

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

        public int GetTicketId(string repo, int prNumber)
        {
            if (string.IsNullOrEmpty(repo) || prNumber < 1)
            {
                return 0;
            }

            var pr = this.GetPRInfo(repo, prNumber);

            if (pr != null)
            {
                var resultString = Regex.Match(pr.head.reference, @"\d+").Value;
                int.TryParse(resultString, out int issueResult);
                return issueResult;
            }

            return 0;
        }

        public PullRequest GetPRInfo(string repo, int prNumber)
        {
            try
            {
                // GET / repos /{ owner}/{ repo}/ pulls /{ pull_number}
                var client = new RestClient($"https://api.github.com/repos/redington/{repo}/pulls/{prNumber}");
                var request = new RestRequest(Method.GET);
                request.AddHeader("authorization", $"token {gitHubAccessToken}");
                var response = client.Execute(request);

                return response.StatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<PullRequest>(response.Content) : null;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<ServiceResult> MergePullRequest(string repo, int prNumber)
        {
            //Get Pull Request 
            var pr = this.GetPRInfo(repo, prNumber);

            if (pr.mergeable_state != "blocked" && !pr.merged && pr.GetMergeDescriptionString() != string.Empty)
            {
                try
                {
                    var result = await this.gitHubApiUrl
                        .AppendPathSegment($"/repos/redington/{repo}/pulls/{prNumber}/merge")
                        .WithHeaders(
                            new
                            {
                                Accept = "application/vnd.github.v3+json",
                                Authorization = $"token {gitHubAccessToken}",
                                User_Agent = "BatonBot"
                            })
                        .PutJsonAsync(
                            new
                            {
                                commit_title = pr.title,
                                commit_message = pr.GetMergeDescriptionString(),
                                merge_method = "squash",
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
                catch (Exception e)
                {
                    return new ServiceResult()
                    {
                        Succeeded = false,
                        ReasonForFailure = e.Message
                    };
                }
            }
            return new ServiceResult()
            {
                Succeeded = false,
                ReasonForFailure = $"No clue - state : {pr.mergeable_state}, prMerged: {pr.merged}, desc: {pr.GetMergeDescriptionString()}"
            };
        }

        public async Task<ServiceResult> UpdatePullRequest(string repo, int prNumber)
        {
            var pr = this.GetPRInfo(repo, prNumber);

            if (pr.mergeable_state != "behind")
            {
                return new ServiceResult()
                {
                    Succeeded = false,
                    MergeStatus = pr.mergeable_state,
                    ReasonForFailure = "Not Needed"
                };
            }

            try
            {
                var result = await this.gitHubApiUrl
                    .AppendPathSegment($"/repos/redington/{repo}/pulls/{prNumber}/update-branch")
                    .WithHeaders(
                        new
                        {
                            Accept = "application/vnd.github.lydian-preview+json",
                            Authorization = $"token {this.gitHubAccessToken}",
                            User_Agent = "BatonBot"
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
                        MergeStatus = pr.mergeable_state,
                        ReasonForFailure = result.ResponseMessage.ToString()
                    };
            }
            catch (Exception e)
            {
                return new ServiceResult()
                {
                    Succeeded = false,
                    MergeStatus = pr.mergeable_state,
                    ReasonForFailure = e.Message
                };
            }
        }
    }
}
