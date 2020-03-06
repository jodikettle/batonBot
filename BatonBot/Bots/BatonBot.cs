using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private CallLambdaService service;

        private readonly string[] _batonType = { "be", "fe", "man" };

        public BatonBot(IConfiguration config)
        {
            _appId = config["MicrosoftAppId"];
            _appPassword = config["MicrosoftAppPassword"];
            service = new CallLambdaService();
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var obj = (JObject) turnContext.Activity.Value;
            var text = obj != null ? obj["x"].ToString() : turnContext.Activity.Text.Trim().ToLowerInvariant();

            var batons = service.CallTheLambda().GetAwaiter().GetResult();

            if (text.StartsWith("release "))
            {
                var type = text.Replace("release ", "");
                if (_batonType.Contains(type))
                {
                    await service.TakeBaton(type, null);
                    var activity = MessageFactory.Text($"Baton {type} released. Don't forget to update the board!");
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }
                else
                {
                    var activity = MessageFactory.Text($"Baton {type} is not a thing. But these are {string.Join(',',_batonType)}");
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }
            }
            else if (text.StartsWith("take "))
            {
                var type = text.Replace("take ", "");
                if (_batonType.Contains(type))
                {
                    var baton = batons.FirstOrDefault(x => x.BatonName.Equals(type));

                    if (baton?.Holder == null)
                    {
                        await service.TakeBaton(type, turnContext.Activity.From.Name);
                        var i = batons.IndexOf(baton);
                        if (i == -1)
                        {
                            batons.Add(new BatonModel()
                                {BatonName = type, Holder = turnContext.Activity.From.Name, TakenDate = DateTime.Now});
                        }
                        else
                        {
                            batons[i] = new BatonModel()
                                {BatonName = type, Holder = turnContext.Activity.From.Name, TakenDate = DateTime.Now};
                        }

                        await SendBatonInfoAsync(turnContext, cancellationToken, batons);
                    }
                    else
                    {
                        await this.SendNo(turnContext, cancellationToken, baton.Holder, baton.TakenDate);
                    }
                }
                else
                {
                    var activity = MessageFactory.Text($"Baton {type} is not a thing. But these are {string.Join(',', _batonType)}");
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }
            }
            else if (text.Equals("show baton"))
            {
                await SendBatonInfoAsync(turnContext, cancellationToken, batons);
            }
        }

        private async Task SendNo(ITurnContext turnContext, CancellationToken cancellationToken, string holder, DateTime? dateTaken)
        {
            var reply = MessageFactory.Text($"Sorry I cant give you that baton it is held by {holder} since {dateTaken}.");
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private async Task SendBatonAsync(ITurnContext turnContext,
            CancellationToken cancellationToken, string baton)
        {
            var batons = this.service.CallTheLambda().GetAwaiter().GetResult();
            var card = Cards.Card.CreateAttachment(batons);

            var reply = MessageFactory.Attachment(card);
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private static async Task SendBatonInfoAsync(ITurnContext turnContext,
            CancellationToken cancellationToken,
            List<BatonModel> batons)
        {
            var card = Cards.Card.CreateAttachment(batons);

            var reply = MessageFactory.Attachment(card);
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
