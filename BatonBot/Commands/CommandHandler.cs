using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BatonBot.Firebase;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace BatonBot.Commands
{
    public class CommandHandler: ICommandHandler
    {
        private readonly string[] _batonType = { "be", "fe", "man" };
        private IFirebaseClient client;

        public CommandHandler(IFirebaseClient client)
        {
            this.client = client;
        }

        public async void Handle(string text, string appId, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            if (text.Equals("show baton"))
            {
                new ShowCommandHandler(client).Handler(turnContext, cancellationToken);
            }
            else
            {
                var command = text.Substring(0, text.IndexOf(' '));
                var type = text.Replace(command + " ", "");
                var batonType = checkBatonType(type);
                if (!await CheckBatonIsAThing(batonType, turnContext, cancellationToken))
                {
                    return;
                }

                if (command.Equals("release"))
                {
                    new ReleaseCommandHandler(client, appId).Handler(batonType, turnContext, cancellationToken);
                }
                else if (command.Equals("take"))
                {
                    new TakeCommandHandler(client).Handler(batonType, turnContext, cancellationToken);
                }
                else
                {
                    var activity = MessageFactory.Text($"Huuuhhh?");
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }
            }
        }

        private async Task<bool> CheckBatonIsAThing(string type, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            if (_batonType.Contains(type)) return true;
            var activity = MessageFactory.Text($"Baton {type} is not a thing. But these are {string.Join(',', _batonType)}");
            await turnContext.SendActivityAsync(activity, cancellationToken);
            return false;
        }

        private string checkBatonType(string type)
        {
            if (type.ToLower().Equals("manifest"))
            {
                return "man";
            }
            if (type.ToLower().Equals("backend"))
            {
                return "be";
            }
            return type.ToLower().Equals("frontend") ? "fe" : type;
        }
    }
}
