

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using SharedBaton.Firebase;
using SharedBaton.Interfaces;
using SharedBaton.Models;

namespace SharedBaton.CommandHandlers
{
    using global::Firebase.Database;
    using SharedBaton.Card;
    using SharedBaton.GitHubService;
    using SharedBaton.RepositoryMapper;

    public class ReleaseCommandHandler : IReleaseCommandHandler
    {
        private readonly IFirebaseService service;
        private readonly IRepositoryMapper mapper;
        private readonly string releaseMessageText;
        private readonly IConfiguration config;
        private readonly IFirebaseLogger logger;
        private readonly IWithinReleaseService releaseService;
        private readonly IGitHubService githubService;
        private readonly ICardCreator cardCreator;
        private readonly string appId;

        public ReleaseCommandHandler(IFirebaseService firebaseClient, IConfiguration config, IWithinReleaseService releaseService, IGitHubService githubService, ICardCreator cardCreator, IRepositoryMapper mapper, IFirebaseLogger logger)
        {
            this.service = firebaseClient;
            this.config = config;
            this.releaseMessageText = config["ReleaseBatonText"];
            this.appId = config["MicrosoftAppId"];
            this.releaseService = releaseService;
            this.githubService = githubService;
            this.cardCreator = cardCreator;
            this.logger = logger;
            this.mapper = mapper;
        }

        public async Task Handler(string type, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var baton = await this.service.GetQueueFireObjectForBaton(type);

            if (baton == null) return;

            var queue = baton.Object.Queue;

            var name = turnContext.Activity.From.Name.Replace(" | Redington", "").Replace(" | Godel", "");

            if (queue.Count <= 0) return;

            if (queue.First().UserName.Equals(name))
            {
                await RemoveFromQueueAndInformTheOtherPerson(type, turnContext, baton, queue, name, cancellationToken);
            }
            else
            {
                await RemoveFurtherDownTheList(turnContext, baton, queue, name, cancellationToken);
            }
        }

        public async Task AdminHandler(string type, string nameToRemove, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var baton = await this.service.GetQueueFireObjectForBaton(type);
            if (baton == null) return;
            var queue = baton.Object.Queue;

            if (queue.Count <= 0) return;

            if (queue.First().UserName.ToLower().Equals(nameToRemove.ToLower()))
            {
                await RemoveFromQueueAndInformTheOtherPerson(type, turnContext, baton, queue, nameToRemove, cancellationToken);
            }
            else
            {
                await RemoveFurtherDownTheList(turnContext, baton, queue, nameToRemove, cancellationToken);
            }
        }

        private async Task RemoveFromQueueAndInformTheOtherPerson(string type, ITurnContext<IMessageActivity> turnContext, global::Firebase.Database.FirebaseObject<BatonQueue> baton, Queue<BatonRequest> queue, string name, CancellationToken cancellationToken)
        {
            var oldBatonRequest = queue.Dequeue();

            if (queue.Count > 0)
            {
                //Tell the other person
                await this.Notify(queue.FirstOrDefault(), turnContext);
                queue.FirstOrDefault().DateReceived = DateTime.Now;
            }

            await service.UpdateQueue(baton);

            this.logger.Log(config["AppDisplayName"],
                type, name, oldBatonRequest.DateRequested, oldBatonRequest.DateReceived, DateTime.Now,
                oldBatonRequest.MoveMeCount, false);

            if (oldBatonRequest.PullRequestNumber > 0)
            {
                var reply = MessageFactory.Attachment(new List<Attachment>());
                reply.Attachments.Add(
                    this.cardCreator.DoYouWantToCloseTheTicket(
                            type, this.mapper.GetRepositoryNameFromBatonName(type), oldBatonRequest.PullRequestNumber, githubService)
                        .ToAttachment());

                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
            else
            {
                var activity = MessageFactory.Text(this.releaseMessageText.Replace("{type}", type));
                await turnContext.SendActivityAsync(activity, cancellationToken);
            }
        }

        private async Task RemoveFurtherDownTheList(ITurnContext<IMessageActivity> turnContext, FirebaseObject<BatonQueue> baton, Queue<BatonRequest> queue, string name, CancellationToken cancellationToken)
        {
            baton.Object.Queue = this.removeAnyInQueue(queue, name, turnContext, cancellationToken);

            if (baton.Object.Queue.Count() < queue.Count)
            {
                await service.UpdateQueue(baton);

                var activity = MessageFactory.Text($"I have removed you from the queue");
                await turnContext.SendActivityAsync(activity, cancellationToken);
            }
            else
            {
                await this.SendNotYourBaton(turnContext, cancellationToken);
            }
        }

        private Queue<BatonRequest> removeAnyInQueue(Queue<BatonRequest> batonQueue, string username, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return new Queue<BatonRequest>(batonQueue.Where(x => !x.UserName.Equals(username)));
        }

        private async Task Notify(BatonRequest batonRequest, ITurnContext<IMessageActivity> turnContext)
        {
            if (batonRequest == null || string.IsNullOrEmpty(batonRequest.UserId))
            {
                return;
            }

            if (batonRequest.Conversation != null)
            {
                await ((BotAdapter)turnContext.Adapter).ContinueConversationAsync(this.appId, batonRequest.Conversation, async (context, token) =>
                    await SendYourBatonMessage(batonRequest.BatonName, context, token), default(CancellationToken));

                await this.releaseService.GotBaton(batonRequest, this.appId, true, turnContext, default(CancellationToken));
            }
        }

        private async Task SendYourBatonMessage(string name, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl
            await turnContext.SendActivityAsync($"Hey! its your turn with the {name} baton");
        }

        private async Task SendNotYourBaton(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"I couldn't find you in the list");
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
