using System;

namespace SharedBaton.CommandHandlers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using SharedBaton.Interfaces;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using SharedBaton.Firebase;
    using SharedBaton.Models;

    public class MoveMeCommandHandler : IMoveMeCommandHandler
    {
        private readonly IFirebaseService service;
        private readonly IWithinReleaseService withinRelease;
        private readonly string releaseMessageText;
        private readonly string appId;

        public MoveMeCommandHandler(IFirebaseService firebaseClient, IWithinReleaseService withinRelease, IConfiguration config)
        {
            this.service = firebaseClient;
            this.releaseMessageText = config["MoveMeBatonText"];
            this.withinRelease = withinRelease;
            this.appId = config["MicrosoftAppId"];
        }

        public async Task Handler(string batonName, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var baton = await this.service.GetQueueFireObjectForBaton(batonName);

            if (baton == null) return;

            var queue = baton.Object.Queue;

            var name = turnContext.Activity.From.Name.Replace(" | Redington", "").Replace(" | Godel", "");

            if (queue?.Count <= 0) return;

            // Does the first one belong to that person
            if (queue.First().UserName.Equals(name))
            {
                if (queue.Count == 1)
                {
                    await turnContext.SendActivityAsync($"There's no one else in the Queue");
                }
                else
                {
                    var first = queue.Dequeue();
                    first.Comment = first.Comment + $" {first.UserName} moved down once";
                    first.DateReceived = null;
                    first.MoveMeCount += 1;

                    //Tell the other person
                    await this.Notify(queue.FirstOrDefault(), appId, turnContext);
                    queue.FirstOrDefault().DateReceived = DateTime.Now;

                    var list = queue.ToList();
                    list.Insert(1, first);
                    baton.Object.Queue = new Queue<BatonRequest>(list);

                    await service.UpdateQueue(baton);

                    var activity = MessageFactory.Text(this.releaseMessageText.Replace("{type}", batonName));
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }
            }
            else
            {
                await this.SendNotYourBaton(turnContext, cancellationToken);
                return;
            }
        }
        private async Task SendNotYourBaton(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"Its not your turn");
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private async Task Notify(BatonRequest batonRequest, string appId, ITurnContext<IMessageActivity> turnContext)
        {
            if (batonRequest == null || string.IsNullOrEmpty(batonRequest.UserId))
            {
                return;
            }

            if (batonRequest.Conversation != null)
            {
                await ((BotAdapter)turnContext.Adapter).ContinueConversationAsync(appId, batonRequest.Conversation, async (context, token) =>
                    await SendMessage(batonRequest.BatonName, context, token), default(CancellationToken));

                await this.withinRelease.GotBaton(batonRequest, this.appId, true, turnContext, default(CancellationToken));
            }
        }

        private async Task SendMessage(string name, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            _ = await turnContext.SendActivityAsync($"Hey! its your turn with the {name} baton", cancellationToken: cancellationToken);
        }
    }
}
