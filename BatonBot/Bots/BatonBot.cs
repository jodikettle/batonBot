using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SharedBaton.Interfaces;

namespace BatonBot.Bots
{
    public class BatonBot : ActivityHandler
    {
        private string _appId;
        private string _appPassword;
        private ICommandHandler commandHandler;
        protected readonly ILogger Logger;

        public BatonBot(IConfiguration config, ICommandHandler commandHandler, ILogger<BatonBot> logger)
        {
            this._appId = config["MicrosoftAppId"];
            this._appPassword = config["MicrosoftAppPassword"];
            this.commandHandler = commandHandler;
            this.Logger = logger;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Starting a call with this value{turnContext.Activity.Value} by {turnContext.Activity.From.Name} with id: {turnContext.Activity.From.Id} .");

            var obj = (JObject) turnContext.Activity.Value;
            var text = obj != null ? obj["x"].ToString() : turnContext.Activity.Text.Trim().ToLowerInvariant();

            await commandHandler.Handle(text, this._appId, turnContext, cancellationToken);
        }
    }
}
