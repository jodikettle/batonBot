using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace BatonBot.CommandHandlers
{
    using BatonBot.BatonServices;
    using BatonBot.Firebase;
    using BatonBot.Interfaces;
    using BatonBot.Services.Card;

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
