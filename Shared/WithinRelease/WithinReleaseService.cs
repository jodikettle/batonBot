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

        public async Task GotBaton(BatonRequest baton, string appId, bool notify, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (notify)
            {

                await ((BotAdapter)turnContext.Adapter).ContinueConversationAsync(appId, baton.Conversation, async (context, token) =>
                    await SendYourBatonMessage(baton.BatonName, baton.PullRequestNumber, "Test", context, token), default(CancellationToken));
            }
            else
            {
                var reply1 = MessageFactory.Text(baton.BatonName);
                await turnContext.SendActivityAsync(reply1, cancellationToken);
                var reply2 = MessageFactory.Text(baton.PullRequestNumber.ToString());
                await turnContext.SendActivityAsync(reply2, cancellationToken);
            }

            if (baton.PullRequestNumber == 0)
            {
                return;
            }

            var repoName = getRepo(baton.BatonName);

            if (string.IsNullOrEmpty(repoName))
            {
                return;
            }

            var info = await this.service.GetPRInfo(repoName, baton.PullRequestNumber);

            if (notify)
            {
                await ((BotAdapter)turnContext.Adapter).ContinueConversationAsync(
                    appId, baton.Conversation, async (context, token) =>
                        await SendYourBatonMessage(baton.BatonName, baton.PullRequestNumber, repoName, context, token),
                    default(CancellationToken));
            }
            else
            {
                var reply1 = MessageFactory.Text(info.mergeable_state);
                await turnContext.SendActivityAsync(reply1, cancellationToken);
            }

            if (info.mergeable_state == "blocked")
            {
                if (notify)
                {
                    await ((BotAdapter)turnContext.Adapter).ContinueConversationAsync(
                        appId, baton.Conversation, async (context, token) =>
                            await turnContext.SendActivityAsync($"### Its not ready to go. Do you have enough reviews?"), cancellationToken);
                }
                else
                {
                    var reply = MessageFactory.Text($"### Its not ready to go. Do you have enough reviews?");
                    _ = await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }
            else if (info.mergeable_state == "behind")
            {
                var attachments = new List<Attachment>();
                var reply = MessageFactory.Attachment(attachments);
                reply.Attachments.Add(
                    Card.GetUpdateYourBranchCard(baton.BatonName, repoName, baton.PullRequestNumber)
                        .ToAttachment());

                if (notify)
                {
                    await ((BotAdapter)turnContext.Adapter).ContinueConversationAsync(
                        appId, baton.Conversation, async (context, token) =>
                            await turnContext.SendActivityAsync(reply), cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }
            else 
            {
                var attachments = new List<Attachment>();
                var reply = MessageFactory.Attachment(attachments);
                reply.Attachments.Add(
                    Card.SquashAndMergeCard(baton.BatonName, repoName, baton.PullRequestNumber)
                        .ToAttachment());

                if (notify)
                {
                    await ((BotAdapter)turnContext.Adapter).ContinueConversationAsync(
                        appId, baton.Conversation, async (context, token) =>
                            await turnContext.SendActivityAsync(reply), cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
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

        private async Task SendYourBatonMessage(string name, int PrNumber, string repoName, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl
            await turnContext.SendActivityAsync($"Hey! its your turn with the {name} baton for PR {PrNumber} going to {repoName}");
        }
    }
}
