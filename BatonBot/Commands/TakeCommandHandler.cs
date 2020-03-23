using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BatonBot.Models;
using BatonBot.Services;
using Firebase.Database;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace BatonBot.Commands
{
    public class TakeCommandHandler
    {
        private readonly Firebase service;
        private readonly GetAndDisplayBatonService showBatonService;

        public TakeCommandHandler()
        {
            this.service = new Firebase();
            this.showBatonService = new GetAndDisplayBatonService();
        }

        public async void Handler(string type, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var batons = service.GetQueue().GetAwaiter().GetResult();
            var batonFireObject = batons?.FirstOrDefault(x => x.Object.Name.Equals(type));

            var conversationReference = turnContext.Activity.GetConversationReference();

            if (batonFireObject == null)
            {
                var baton = new BatonQueue(type);

                baton.Queue.Enqueue(new BatonRequest()
                {
                    UserName = turnContext.Activity.From.Name,
                    UserId = conversationReference.User.Id,
                    DateRequested = DateTime.Now,
                    Conversation = conversationReference
                });

                service.SaveQueue(baton);

                await SendItsAllYours(turnContext, cancellationToken);
            }
            else
            {
                 if (batonFireObject.Object.Queue.Count == 0)
                 {
                     await this.SendItsAllYours(turnContext, cancellationToken);
                 }
                 else
                 {
                     await this.SendAddedToTheQueue(turnContext, cancellationToken);
                 }

                 batonFireObject.Object.Queue.Enqueue(new BatonRequest()
                     {UserName = turnContext.Activity.From.Name, UserId = conversationReference.User.Id, DateRequested = DateTime.Now, Conversation = conversationReference });
                 service.UpdateQueue(batonFireObject);

                 await showBatonService.SendBatons(turnContext, cancellationToken);
            }
        }

        private async Task SendAddedToTheQueue(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"I have added you to the queue");
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private async Task SendItsAllYours(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"Its all yours");
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
