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
        private readonly string releaseMessageText;

        public MoveMeCommandHandler(IFirebaseService firebaseClient, IConfiguration config)
        {
            this.service = firebaseClient;
            this.releaseMessageText = config["MoveMeBatonText"];
        }

        public async Task Handler(string type, string appId, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var batons = service.GetQueue().GetAwaiter().GetResult();
            var batonFireObject = batons?.FirstOrDefault(x => x.Object.Name.Equals(type));

            if (batonFireObject == null) return;

            var queue = batonFireObject.Object.Queue;

            var name = turnContext.Activity.From.Name.Replace(" | Redington", "").Replace(" | Godel", "");

            if (queue.Count <= 0) return;

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

                    //Tell the other person
                    await this.Notify(queue.FirstOrDefault(), appId, turnContext);
                    queue.FirstOrDefault().DateReceived = DateTime.Now;

                    var list = queue.ToList();
                    list.Insert(1, first);

                    batonFireObject.Object.Queue = new Queue<BatonRequest>(list);


                    await service.UpdateQueue(batonFireObject);

                    var activity = MessageFactory.Text(this.releaseMessageText.Replace("{type}", type));
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

        private async Task BotCallback(string name, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl
            await turnContext.SendActivityAsync($"Hey! its your turn with the {name} baton");
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
                    await BotCallback(batonRequest.BatonName, context, token), default(CancellationToken));
            }
        }
    }
}
