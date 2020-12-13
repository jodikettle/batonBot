using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using SharedBaton.Firebase;
using SharedBaton.Models;

namespace DevEnvironmentBot.Commands
{
    public class ReleaseCommandHandler
    {
        private readonly IFirebaseService service;
        private readonly string appId;

        public ReleaseCommandHandler(IFirebaseService firebaseClient, string appId)
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

            var name = turnContext.Activity.From.Name.Replace(" | Redington", "").Replace(" | Godel", "");

            if (queue.Count <= 0) return;

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

                var activity1 = MessageFactory.Text($"Releasing");
                await turnContext.SendActivityAsync(activity1, cancellationToken);

                await service.UpdateQueue(batonFireObject);

                var activity = MessageFactory.Text($"Baton {type} released.");
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
