using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharedBaton.Firebase;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using SharedBaton.Commands;
using DevEnvironmentBot.Batons;
using SharedBaton.Card;

namespace DevEnvironmentBot.Commands
{
    public class CommandHandler: ICommandHandler
    {
        private ICardCreator cardCreator;
        private IFirebaseService client;
        private BatonService batons;

        public CommandHandler(IFirebaseService client, ICardCreator cardCreator)
        {
            this.client = client;
            this.batons = new BatonService();
            this.cardCreator = cardCreator;
        }

        public async void Handle(string text, string appId, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            if (text.Equals("show baton"))
            {
                new ShowCommandHandler(client, cardCreator).Handler(turnContext, cancellationToken);
            }
            else
            {
                var command = text.Substring(0, text.IndexOf(' '));
                var type = text.Replace(command + " ", "");
                var batonType = batons.checkBatonType(type);
                if (!await CheckBatonIsAThing(batonType, turnContext, cancellationToken))
                {
                    return;
                }

                if (command.Equals("release"))
                {
                    new ReleaseCommandHandler(client, appId).Handler(batonType.Shortname, turnContext, cancellationToken);
                }
                else if (command.Equals("take"))
                {
                    new TakeCommandHandler(client, cardCreator).Handler(batonType.Shortname, turnContext, cancellationToken);
                }
                else
                {
                    var activity = MessageFactory.Text($"Huuuhhh?");
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }
            }
        }

        private async Task<bool> CheckBatonIsAThing(Baton type, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            if (type != null) return true;
            var activity = MessageFactory.Text($"Baton {type} is not a thing. But these are {this.batons.List()}");
            await turnContext.SendActivityAsync(activity, cancellationToken);

            return false;
        }
    }
}
