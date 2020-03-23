using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BatonBot.Commands;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace BatonBot.Bots
{
    public class BatonBot : TeamsActivityHandler
    {
        private string _appId;
        private string _appPassword;
        private ReleaseCommandHandler releaseHandler;
        private ShowCommandHandler showHandler;
        private TakeCommandHandler takeHandler;

        private readonly string[] _batonType = { "be", "fe", "man" };

        public BatonBot(IConfiguration config)
        {
            _appId = config["MicrosoftAppId"];
            _appPassword = config["MicrosoftAppPassword"];
            releaseHandler = new ReleaseCommandHandler(_appId);
            showHandler = new ShowCommandHandler();
            takeHandler = new TakeCommandHandler();
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var obj = (JObject) turnContext.Activity.Value;
            var text = obj != null ? obj["x"].ToString() : turnContext.Activity.Text.Trim().ToLowerInvariant();

            if (text.StartsWith("release "))
            {
                var type = text.Replace("release ", "");
                if (await CheckBatonIsAThing(type, turnContext, cancellationToken))
                    releaseHandler.Handler(type, turnContext, cancellationToken);
            }
            else if (text.StartsWith("take "))
            {
                var type = text.Replace("take ", "");
                if (await CheckBatonIsAThing(type, turnContext, cancellationToken))
                    takeHandler.Handler(type, turnContext, cancellationToken);
            }
            else if (text.Equals("show baton"))
            {
                showHandler.Handler(turnContext, cancellationToken);
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
    }
}
