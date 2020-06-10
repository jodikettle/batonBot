using System.Threading;
using BatonBot.Firebase;
using BatonBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace BatonBot.Commands
{
    public class ShowCommandHandler
    {
        private readonly GetAndDisplayBatonService service;

        public ShowCommandHandler(IFirebaseClient client)
        {
            this.service = new GetAndDisplayBatonService(client);
        }

        public async void Handler(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            await this.service.SendBatons(turnContext, cancellationToken);
        }
    }
}
