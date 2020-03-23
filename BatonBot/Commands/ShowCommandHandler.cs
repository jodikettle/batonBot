using System.Threading;
using BatonBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace BatonBot.Commands
{
    public class ShowCommandHandler
    {
        private readonly GetAndDisplayBatonService service;

        public ShowCommandHandler()
        {
            this.service = new GetAndDisplayBatonService();
        }

        public async void Handler(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            await this.service.SendBatons(turnContext, cancellationToken);
        }
    }
}
