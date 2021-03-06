﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SharedBaton.Interfaces;

namespace BatonBot.Bots
{
    public class BatonBot : TeamsActivityHandler
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

        protected override async Task OnTeamsMembersAddedAsync(IList<TeamsChannelAccount> teamsMembersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (TeamsChannelAccount member in teamsMembersAdded)
            {
                if (member.Id == turnContext.Activity.Recipient.Id)
                {
                    // Send a message to introduce the bot to the team
                    var heroCard = new HeroCard(text: $"Hi, I've joined in so that you can share the baton queue in this chat just call me @BatonBot show baton");
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
                }
            }
        }
    }
}
