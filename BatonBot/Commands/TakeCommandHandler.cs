using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BatonBot.Firebase;
using BatonBot.Models;
using BatonBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace BatonBot.Commands
{
    public class TakeCommandHandler : ITakeCommandHandler
    {
        private readonly IFirebaseClient service;
        private readonly GetAndDisplayBatonService showBatonService;

        public TakeCommandHandler(IFirebaseClient firebaseService)
        {
            this.service = firebaseService;
            this.showBatonService = new GetAndDisplayBatonService(firebaseService);
        }

        public async void Handler(string type, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var batons = service.GetQueue().GetAwaiter().GetResult();
            var batonFireObject = batons?.FirstOrDefault(x => x.Object.Name.Equals(type));

            var conversationReference = turnContext.Activity.GetConversationReference();

            var name = turnContext.Activity.From.Name.Replace(" | Redington", "");

            if (batonFireObject == null)
            {
                var baton = new BatonQueue(type);

                baton.Queue.Enqueue(new BatonRequest()
                {
                    UserName = name,
                    UserId = conversationReference.User.Id,
                    DateRequested = DateTime.Now.ToLocalTime(),
                    DateReceived = DateTime.Now.ToLocalTime(),
                    Conversation = conversationReference
                });

                service.SaveQueue(baton);

                await SendItsAllYours(turnContext, cancellationToken);
            }
            else
            {
                 if (batonFireObject.Object.Queue.Count == 0)
                 {
                     batonFireObject.Object.Queue.Enqueue(new BatonRequest()
                         { UserName = name, UserId = conversationReference.User.Id, DateRequested = DateTime.Now, DateReceived = DateTime.Now, Conversation = conversationReference });

                    await this.SendItsAllYours(turnContext, cancellationToken);
                 }
                 else
                 {
                     batonFireObject.Object.Queue.Enqueue(new BatonRequest()
                         { UserName = name, UserId = conversationReference.User.Id, DateRequested = DateTime.Now, Conversation = conversationReference });

                    await this.SendAddedToTheQueue(turnContext, cancellationToken);
                 }

                 await service.UpdateQueue(batonFireObject);

                 //TODO - just use the queue you have
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
