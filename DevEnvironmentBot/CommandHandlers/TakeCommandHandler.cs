
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace BatonBot.CommandHandlers
{
    using BatonBot.BatonServices;
    using BatonBot.Commands;
    using BatonBot.Firebase;
    using BatonBot.Interfaces;
    using BatonBot.Models;
    using BatonBot.Services.Card;

    public class TakeCommandHandler: ITakeCommandHandler
    {
        private readonly IFirebaseService service;
        private readonly GetAndDisplayBatonService showBatonService;
        private readonly IWithinReleaseService releaseService;

        public TakeCommandHandler(IFirebaseService firebaseService, ICardCreator cardBuilder, IWithinReleaseService releaseService)
        {
            this.service = firebaseService;
            this.showBatonService = new GetAndDisplayBatonService(firebaseService, cardBuilder);
            this.releaseService = releaseService;
        }
        public async Task Handler(string type, string comment, int pullRequest, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var batons = await service.GetQueues();
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
                    BatonName = type,
                    Comment = comment,
                    PullRequestNumber = pullRequest
                });

                service.SaveQueue(baton);

                SendItsAllYours(turnContext, cancellationToken);
            }
            else
            {
                if (batonFireObject.Object.Queue.Count == 0)
                {
                    var baton = new BatonRequest()
                        {
                            UserName = name,
                            UserId = conversationReference.User.Id,
                            BatonName = type,
                            DateRequested = DateTime.Now,
                            DateReceived = DateTime.Now,
                            Conversation = conversationReference,
                            Comment = comment,
                            PullRequestNumber = pullRequest
                        };

                    batonFireObject.Object.Queue.Enqueue(baton);

                    await service.UpdateQueue(batonFireObject);

                    this.SendItsAllYours(turnContext, cancellationToken);
                    await this.releaseService.GotBaton(baton, string.Empty, false, turnContext, cancellationToken);
                }
                else
                {
                    batonFireObject.Object.Queue.Enqueue(new BatonRequest()
                    { UserName = name, UserId = conversationReference.User.Id, BatonName = type, DateRequested = DateTime.Now, Conversation = conversationReference, Comment = comment, PullRequestNumber = pullRequest });

                    await service.UpdateQueue(batonFireObject);

                    await this.SendAddedToTheQueue(turnContext, cancellationToken);
                    //TODO - just use the queue you have
                    await showBatonService.SendBatons(turnContext, cancellationToken);
                }
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
