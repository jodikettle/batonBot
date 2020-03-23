using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BatonBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace BatonBot.Commands
{
    public class ReleaseCommandHandler
    {
        private readonly Firebase service;
        private readonly string appId;

        public ReleaseCommandHandler(string appId)
        {
            service = new Firebase();
            this.appId = appId;
        }

        public async void Handler(string type, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var batons = service.GetQueue().GetAwaiter().GetResult();
            var batonFireObject = batons?.FirstOrDefault(x => x.Object.Name.Equals(type));

            if (batonFireObject != null)
            {
                var queue = batonFireObject.Object.Queue;

                if (queue.Count > 0)
                {
                    // Does the first one belong to that person
                    if (queue.First().UserName == turnContext.Activity.From.Name)
                    {
                        queue.Dequeue();
                        service.UpdateQueue(batonFireObject);

                        if (queue.Count > 0)
                        {
                            //Tell the other person
                            await this.Notify(queue.FirstOrDefault(), turnContext);
                        }
                    }
                    else
                    {
                       await this.SendNotYourBaton(turnContext, cancellationToken);
                    }
                }
            }

            var activity = MessageFactory.Text($"Baton {type} released. Don't forget to update the board!");
            await turnContext.SendActivityAsync(activity, cancellationToken);
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
            var reply = MessageFactory.Text($"You don't even have the baton");
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
