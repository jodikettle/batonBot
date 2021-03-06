﻿namespace BatonBot.CommandHandlers
{
    using System.Linq;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using System.Threading;
    using System.Threading.Tasks;
    using BatonBot.Firebase;
    using BatonBot.GitHubService;
    using BatonBot.Interfaces;
    using BatonBot.Services.RepositoryMapper;

    public class MergeGithubHandler : IGithubMergeHandler {

        private readonly IGitHubService gitHubService;
        private readonly IFirebaseService service;
        private readonly IRepositoryMapper repoMapper;

        public MergeGithubHandler(IGitHubService gitHubService, IFirebaseService service, IRepositoryMapper repoMapper)
        {
            this.gitHubService = gitHubService;
            this.service = service;
            this.repoMapper = repoMapper;
        }

        public async Task Handler(string batonName, int prNumber, string appId, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var queue = await service.GetQueueForBaton(batonName);

            var name = turnContext.Activity.From.Name.Replace(" | Redington", "").Replace(" | Godel", "");

            if (queue?.Count <= 0) return;

            var batonPrRequest = queue.FirstOrDefault(x => x.UserName.Equals(name));

            if (batonPrRequest.PullRequestNumber > 0)
            {
                var repo = this.repoMapper.GetRepositoryNameFromBatonName(batonName);

                if (!string.IsNullOrEmpty(repo))
                {
                    var result = await this.gitHubService.MergePullRequest(repo, batonPrRequest.PullRequestNumber);

                    if (result.Succeeded)
                    {
                        var reply = MessageFactory.Text($"That has been merged");
                        _ = await turnContext.SendActivityAsync(reply, cancellationToken);
                    }
                    else
                    {
                        var reply = MessageFactory.Text($"Something went wrong - {result.ReasonForFailure}");
                        _ = await turnContext.SendActivityAsync(reply, cancellationToken);
                    }
                }
            }
        }
    }
}
