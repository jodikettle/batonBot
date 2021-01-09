using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using SharedBaton.BatonServices;
using SharedBaton.Card;
using SharedBaton.Firebase;
using SharedBaton.Interfaces;

namespace SharedBaton.CommandHandlers
{
    public class ShowCommandHandler : IShowCommandHandler
    {
        private readonly GetAndDisplayBatonService service;

        public ShowCommandHandler(IFirebaseService client, ICardCreator cardCreator)
        {
            this.service = new GetAndDisplayBatonService(client, cardCreator);
        }

        public async void Handler(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            await this.service.SendBatons(turnContext, cancellationToken);
        }
    }
}
