using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BatonBot.Commands;
using BatonBot.Firebase;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace BatonBot.Bots
{
    public class BatonBot : ActivityHandler
    {
        private string _appId;
        private string _appPassword;
        private ICommandHandler commandHandler;

        public BatonBot(IConfiguration config, ICommandHandler commandHandler)
        {
            this._appId = config["MicrosoftAppId"];
            this._appPassword = config["MicrosoftAppPassword"];
            this.commandHandler = commandHandler;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var obj = (JObject) turnContext.Activity.Value;
            var text = obj != null ? obj["x"].ToString() : turnContext.Activity.Text.Trim().ToLowerInvariant();

            commandHandler.Handle(text, this._appId, turnContext, cancellationToken);
        }
    }
}
