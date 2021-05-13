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

    public class MergeGithubHandler : IGithubMergeHandler {

        private readonly IGitHubService gitHubService;
        private readonly IFirebaseService service;

        public MergeGithubHandler(IGitHubService gitHubService, IFirebaseService service)
        {
            this.gitHubService = gitHubService;
            this.service = service;
        }

        public async Task Handler(string type, int prNumber, string appId, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var batons = service.GetQueue().GetAwaiter().GetResult();
            var batonFireObject = batons?.FirstOrDefault(x => x.Object.Name.Equals(type));

            if (batonFireObject == null) return;

            var queue = batonFireObject.Object.Queue;

            var name = turnContext.Activity.From.Name.Replace(" | Redington", "").Replace(" | Godel", "");

            if (queue.Count <= 0) return;

            var batonPrRequest = queue.FirstOrDefault(x => x.UserName.Equals(name));

            if (batonPrRequest.PullRequestNumber > 0)
            {
                var repo = string.Empty;

                if (type == "be")
                {
                    repo = "maraschino";
                }

                if (type == "fe")
                {
                    repo = "ADA-Research-UI";
                }

                if (type == "man")
                {
                    repo = "ADA-Research-Configuration";
                }

                if (!string.IsNullOrEmpty(repo))
                {
                    var result = await this.gitHubService.MergePullRequest(repo, batonPrRequest.PullRequestNumber);

                    if (result.Succeeded)
                    {
                        var reply = MessageFactory.Text($"Thats been merged");
                        _ = await turnContext.SendActivityAsync(reply, cancellationToken);
                    }
                    else if (result.ReasonForFailure == "No clue")
                    {
                        var reply = MessageFactory.Text($"Something went wrong no idea what");
                        _ = await turnContext.SendActivityAsync(reply, cancellationToken);
                    }
                    else
                    {
                        var reply = MessageFactory.Text($"Somethign went wrong - {result.ReasonForFailure}");
                        _ = await turnContext.SendActivityAsync(reply, cancellationToken);
                    }
                }
            }
        }
    }
}
