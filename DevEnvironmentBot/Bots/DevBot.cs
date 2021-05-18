
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using BatonBot.Interfaces;

namespace BatonBot.Bots
{
    public class DevBot : TeamsActivityHandler
    {
        private readonly string appId;
        private readonly string appName;
        private readonly ICommandHandler commandHandler;
        private readonly BotState userState;

        public DevBot(IConfiguration config, ICommandHandler commandHandler, UserState userState)
        {
            this.appId = config["MicrosoftAppId"];
            this.appName = config["AppDisplayName"];
            this.commandHandler = commandHandler;
            this.userState = userState;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            await this.userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var obj = (JObject) turnContext.Activity.Value;
            var text = obj != null ? obj["x"].ToString() : turnContext.Activity.Text.Trim().ToLowerInvariant();

            await this.commandHandler.Handle(text, this.appId, turnContext, cancellationToken);
        }

        protected override async Task OnTeamsMembersAddedAsync(IList<TeamsChannelAccount> teamsMembersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (TeamsChannelAccount member in teamsMembersAdded)
            {
                if (member.Id == turnContext.Activity.Recipient.Id)
                {
                    // Send a message to introduce the bot to the team
                    var heroCard = new HeroCard(text: $"Hi, I've joined in so that you can share the {this.appName} baton queue in this chat just call me @{this.appName} show baton");
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
                }
            }
        }
    }
}
