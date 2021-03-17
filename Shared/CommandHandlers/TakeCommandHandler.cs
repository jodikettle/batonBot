
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using SharedBaton.BatonServices;
using SharedBaton.Card;
using SharedBaton.Commands;
using SharedBaton.Firebase;
using SharedBaton.Models;

namespace SharedBaton.CommandHandlers
{
    public class TakeCommandHandler: ITakeCommandHandler
    {
        private readonly IFirebaseService service;
        private readonly GetAndDisplayBatonService showBatonService;

        public TakeCommandHandler(IFirebaseService firebaseService, ICardCreator cardBuilder)
        {
            this.service = firebaseService;
            this.showBatonService = new GetAndDisplayBatonService(firebaseService, cardBuilder);
        }
        public async Task Handler(string type, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var batons = service.GetQueue().GetAwaiter().GetResult();
            var batonFireObject = batons?.FirstOrDefault(x => x.Object.Name.Equals(type));

            var conversationReference = turnContext.Activity.GetConversationReference();

            var name = turnContext.Activity.From.Name.Replace(" | Redington", "").Replace(" | Godel", "");
            if (batonFireObject == null)
            {
                var baton = new BatonQueue(type);

                baton.Queue.Enqueue(new BatonRequest()
                {
                    UserName = name,
                    UserId = conversationReference.User.Id,
                    DateRequested = DateTime.Now.ToLocalTime(),
                    DateReceived = DateTime.Now.ToLocalTime(),
                    Conversation = conversationReference,
                    BatonName = type
                });

                service.SaveQueue(baton);

                SendItsAllYours(turnContext, cancellationToken);
            }
            else
            {
                if (batonFireObject.Object.Queue.Count == 0)
                {
                    batonFireObject.Object.Queue.Enqueue(new BatonRequest()
                    { UserName = name, UserId = conversationReference.User.Id, BatonName = type, DateRequested = DateTime.Now, DateReceived = DateTime.Now, Conversation = conversationReference });

                    this.SendItsAllYours(turnContext, cancellationToken);
                }
                else
                {
                    batonFireObject.Object.Queue.Enqueue(new BatonRequest()
                    { UserName = name, UserId = conversationReference.User.Id, BatonName = type, DateRequested = DateTime.Now, Conversation = conversationReference });

                    await this.SendAddedToTheQueue(turnContext, cancellationToken);
                }

                await service.UpdateQueue(batonFireObject);


                //////TODO - just use the queue you have
                await showBatonService.SendBatons(turnContext, cancellationToken);
            }
        }

        private async Task SendAddedToTheQueue(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"I have added you to the queue");
            _ = await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private void SendItsAllYours(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"Its all yours");
            turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
