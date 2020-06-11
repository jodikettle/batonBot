using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BatonBot.Firebase;
using BatonBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace BatonBot.Commands
{
    public class ReleaseCommandHandler
    {
        private readonly IFirebaseClient service;
        private readonly string appId;

        public ReleaseCommandHandler(IFirebaseClient firebaseClient, string appId)
        {
            this.service = firebaseClient;
            this.appId = appId;
        }

        public async void Handler(string type, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var batons = service.GetQueue().GetAwaiter().GetResult();
            var batonFireObject = batons?.FirstOrDefault(x => x.Object.Name.Equals(type));

            if (batonFireObject == null) return;

            var queue = batonFireObject.Object.Queue;

            var name = turnContext.Activity.From.Name.Replace(" | Redington", "");

            if (queue.Count <= 0) TEST return;

            // Does the first one belong to that person
            if (queue.First().UserName.Equals(name))
            {
                queue.Dequeue();

                if (queue.Count > 0)
                {
                    //Tell the other person
                    await this.Notify(queue.FirstOrDefault(), turnContext);
                    queue.FirstOrDefault().DateReceived = DateTime.Now;
                }

                service.UpdateQueue(batonFireObject);

                var activity = MessageFactory.Text($"Baton {type} released. Now its time to move your ticket into closed on the zenhub board");
                await turnContext.SendActivityAsync(activity, cancellationToken);
            }
            else
            {
                var queueLength = queue.Count;
                batonFireObject.Object.Queue = this.removeAnyInQueue(queue, name, turnContext, cancellationToken);

                if (batonFireObject.Object.Queue.Count() < queueLength)
                {
                    service.UpdateQueue(batonFireObject);

                    var activity = MessageFactory.Text($"I have removed you from the queue");
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }
                else
                {
                    await this.SendNotYourBaton(turnContext, cancellationToken);
                }
                return;
            }
        }

        private Queue<BatonRequest> removeAnyInQueue(Queue<BatonRequest> batonQueue, string username, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var name = username.Replace(" | Redington", "");

            return new Queue<BatonRequest>(batonQueue.Where(x => !x.UserName.Equals(name)));
        }

        private async Task Notify(BatonRequest batonRequest, ITurnContext<IMessageActivity> turnContext)
        {
            if (batonRequest == null || string.IsNullOrEmpty(batonRequest.UserId))
            {
                return;
            }

            if (batonRequest.Conversation != null)
            {
                await ((BotAdapter)turnContext.Adapter).ContinueConversationAsync(appId, batonRequest.Conversation, BotCallback, default(CancellationToken));
            }
        }

        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl
            await turnContext.SendActivityAsync("Hey! its your turn with the baton");
        }

        private async Task SendNotYourBaton(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"I couldn't find you in the list");
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
