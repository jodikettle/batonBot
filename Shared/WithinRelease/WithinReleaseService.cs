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
                var reply1 = MessageFactory.Text("MergeState:"+ info.mergeable_state);

                await ((BotAdapter)turnContext.Adapter).ContinueConversationAsync(
                    appId, baton.Conversation, async (context, token) =>
                        await SendMergeMessage(reply1, context, token),
                    default(CancellationToken));

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

            if (info.mergeable_state == "dirty")
            {
                if (notify)
                {
                    await ((BotAdapter)turnContext.Adapter).ContinueConversationAsync(
                        appId, baton.Conversation, async (context, token) =>
                            await this.SendMergeMessage($"### Cant merge. this requires attention", context, token),
                        default(CancellationToken));
                }
                else
                {
                    var reply = MessageFactory.Text($"### Cant merge. this is required attention");
                    _ = await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }
            else if (info.mergeable_state == "blocked")
            {
                if (notify)
                {
                    await ((BotAdapter)turnContext.Adapter).ContinueConversationAsync(
                        appId, baton.Conversation, async (context, token) =>
                            await this.SendMergeMessage($"### Its not ready to go. Do you have enough reviews?", context, token),
                        default(CancellationToken));
                }
                else
                {
                    var reply = MessageFactory.Text($"### Its not ready to go. Do you have enough reviews?");
                    _ = await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }
            else if (info.mergeable_state == "behind")
            {
                var reply = MessageFactory.Attachment(new List<Attachment>());
                reply.Attachments.Add(
                    Card.GetUpdateYourBranchCardBeforeMerge(baton.BatonName, repoName, baton.PullRequestNumber)
                        .ToAttachment());

                if (notify)
                {
                    await ((BotAdapter)turnContext.Adapter).ContinueConversationAsync(
                        appId, baton.Conversation, async (context, token) =>
                            await this.SendMergeMessage(reply, context, token),
                        default(CancellationToken));
                }
                else
                {
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }
            else 
            {
                var reply = MessageFactory.Attachment(new List<Attachment>());
                reply.Attachments.Add(
                    Card.SquashAndMergeCard(baton.BatonName, repoName, baton.PullRequestNumber)
                        .ToAttachment());

                if (notify)
                {
                    await ((BotAdapter)turnContext.Adapter).ContinueConversationAsync(
                        appId, baton.Conversation, async (context, token) =>
                            await this.SendMergeMessage(reply, context, token),
                        default(CancellationToken));
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

        private async Task SendMergeMessage(string message, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl
            await turnContext.SendActivityAsync(message);
        }

        private async Task SendMergeMessage(IMessageActivity message, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl
            await turnContext.SendActivityAsync(message);
        }

        private async Task SendYourBatonMessage(string name, int PrNumber, string repoName, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl
            await turnContext.SendActivityAsync($"Hey! its your turn with the {name} baton for PR {PrNumber} going to {repoName}");
        }
    }
}
