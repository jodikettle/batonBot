namespace SharedBaton.WithinRelease
{
    using System.Collections.Generic;
    using System.Threading;
    using SharedBaton.Interfaces;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using SharedBaton.Card;
    using SharedBaton.GitHubService;
    using SharedBaton.Models;
    using SharedBaton.RepositoryMapper;

    public class WithinReleaseService : IWithinReleaseService
    {
        private readonly IGitHubService service;
        private readonly ICardCreator cardCreator;
        private readonly IRepositoryMapper repoMapper;

        public WithinReleaseService(IGitHubService service, ICardCreator cardCreator, IRepositoryMapper repoMapper)
        {
            this.service = service;
            this.cardCreator = cardCreator;
            this.repoMapper = repoMapper;
        }

        public async Task GotBaton(BatonRequest baton, string appId, bool notify, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (baton.PullRequestNumber == 0)
            {
                return;
            }

            var repoName = this.repoMapper.GetRepositoryNameFromBatonName(baton.BatonName);

            if (string.IsNullOrEmpty(repoName))
            {
                return;
            }

            var info = this.service.GetPRInfo(repoName, baton.PullRequestNumber);

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
                var reply1 = MessageFactory.Text("merge status:" + info.mergeable_state);
                await turnContext.SendActivityAsync(reply1, cancellationToken);
            }

            if (info.mergeable_state == "dirty")
            {
                if (notify)
                {
                    await ((BotAdapter)turnContext.Adapter).ContinueConversationAsync(
                        appId, baton.Conversation, async (context, token) =>
                            await this.SendMergeMessage($"### Cant merge. It requires attention", context, token),
                        default(CancellationToken));
                }
                else
                {
                    var reply = MessageFactory.Text($"### Cant merge. It requires attention");
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
                    cardCreator.GetUpdateYourBranchCardBeforeMerge(baton.BatonName, repoName, baton.PullRequestNumber)
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
            else if (info.mergeable_state == "clean")
            {
                var reply = MessageFactory.Attachment(new List<Attachment>());
                reply.Attachments.Add(
                    cardCreator.SquashAndMergeCard(baton.BatonName, repoName, baton.PullRequestNumber)
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
                if (notify)
                {
                    await ((BotAdapter)turnContext.Adapter).ContinueConversationAsync(
                        appId, baton.Conversation, async (context, token) =>
                            await this.SendMergeMessage($"Something went wrong the merge status is {info.mergeable_state}", context, token),
                        default(CancellationToken));
                }
                else
                {
                    var reply = MessageFactory.Text($"Something went wrong the merge status is {info.mergeable_state}");
                    _ = await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }
        }

        private async Task SendMergeMessage(string message, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            _ = await turnContext.SendActivityAsync(message, cancellationToken: cancellationToken);
        }

        private async Task SendMergeMessage(IMessageActivity message, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            _ = await turnContext.SendActivityAsync(message, cancellationToken);
        }

        //This can go after testing
        private async Task SendYourBatonMessage(string name, int PrNumber, string repoName, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl
            _ = await turnContext.SendActivityAsync($"Hey! its your turn with the {name} baton for PR {PrNumber} going to {repoName}", cancellationToken: cancellationToken);
        }
    }
}
