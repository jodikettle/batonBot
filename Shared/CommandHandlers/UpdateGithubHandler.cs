namespace SharedBaton.CommandHandlers
{
    using System.Linq;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using SharedBaton.GitHubService;
    using SharedBaton.Interfaces;
    using System.Threading;
    using System.Threading.Tasks;
    using SharedBaton.Firebase;
    using SharedBaton.RepositoryMapper;

    public class UpdateGithubHandler : IGithubUpdateHandler {

        private readonly IGitHubService gitHubService;
        private readonly IFirebaseService service;
        private readonly IRepositoryMapper repoMapper;

        public UpdateGithubHandler(IGitHubService gitHubService, IFirebaseService service, IRepositoryMapper repoMapper)
        {
            this.gitHubService = gitHubService;
            this.service = service;
            this.repoMapper = repoMapper;
        }

        public async Task Handler(string batonName, int pr, string appId, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var name = turnContext.Activity.From.Name.Replace(" | Redington", "").Replace(" | Godel", "");

            var queue =  await this.service.GetQueueForBaton(batonName);
            if (queue?.Count <= 0) return;

            var batonPrRequest = queue.FirstOrDefault(x => x.UserName.Equals(name));

            if (batonPrRequest.PullRequestNumber > 0)
            {
                var repo = this.repoMapper.GetRepositoryNameFromBatonName(batonName);

                if (!string.IsNullOrEmpty(repo))
                {
                    var result = await this.gitHubService.UpdatePullRequest(repo, batonPrRequest.PullRequestNumber);

                    if (result.Succeeded)
                    {
                        var reply = MessageFactory.Text($"Im Updating that for you");
                        _ = await turnContext.SendActivityAsync(reply, cancellationToken);
                    }
                    else if (result.ReasonForFailure == "Not Needed")
                    {
                        var reply = MessageFactory.Text($"Its not required to update this branch at this time. {result.MergeStatus}");
                        _ = await turnContext.SendActivityAsync(reply, cancellationToken);
                    }
                    else
                    {
                        var reply = MessageFactory.Text($"That didn't work out can you update it on the link");
                        _ = await turnContext.SendActivityAsync(reply, cancellationToken);

                        var reply1 = MessageFactory.Text($"{result.MergeStatus} - {result.ReasonForFailure}");
                        _ = await turnContext.SendActivityAsync(reply1, cancellationToken);
                    }
                }
            }
        }
    }
}
