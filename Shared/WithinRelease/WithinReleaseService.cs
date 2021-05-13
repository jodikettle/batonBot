namespace SharedBaton.WithinRelease
{
    using System.Collections.Generic;
    using System.Threading;
    using SharedBaton.Interfaces;
    using System.Threading.Tasks;
    using DevEnvironmentBot.Cards;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using SharedBaton.GitHubService;
    using SharedBaton.Models;

    public class WithinReleaseService : IWithinReleaseService
    {
        public IGitHubService service;

        public WithinReleaseService(IGitHubService service)
        {
            this.service = service;
        }

        public async Task GotBaton(BatonRequest baton, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (baton.PullRequestNumber == 0)
            {
                return;
            }

            var repoName = getRepo(baton.BatonName);

            if (string.IsNullOrEmpty(repoName))
            {
                return;
            }

            var info = await this.service.getPRInfo(repoName, baton.PullRequestNumber);

            if (info.mergeable_state == "blocked")
            {
                var reply = MessageFactory.Text($"### Its not ready to go. Do you have enough reviews?");
                _ = await turnContext.SendActivityAsync(reply, cancellationToken);
            }
            else if (info.mergeable_state == "behind")
            {
                var attachments = new List<Attachment>();
                var reply = MessageFactory.Attachment(attachments);
                reply.Attachments.Add(
                    Card.GetUpdateYourBranchCard(baton.BatonName, repoName, baton.PullRequestNumber)
                        .ToAttachment());

                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
            else 
            {
                var attachments = new List<Attachment>();
                var reply = MessageFactory.Attachment(attachments);
                reply.Attachments.Add(
                    Card.SquashAndMergeCard(baton.BatonName, repoName, baton.PullRequestNumber)
                        .ToAttachment());

                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
        }

        private string getRepo(string type)
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
            return repo;
        }
    }
}
