using Microsoft.AspNetCore.Mvc;

namespace DevEnvironmentBot.Controllers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DevEnvironmentBot.Cards;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Configuration;
    using SharedBaton.Firebase;

    [Route("api/[controller]")]
    [ApiController]
    public class SomeonesMergedController : ControllerBase
    {
        private readonly IFirebaseService service;
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly int _timerCheckAllowance;

        public SomeonesMergedController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, IFirebaseService firebaseClient, ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _adapter = adapter;
            _conversationReferences = conversationReferences;
            _appId = configuration["MicrosoftAppId"];
            _timerCheckAllowance = Int32.Parse(configuration["TimerCheckAllowance"]);

            // If the channel is the Emulator, and authentication is not in use,
            // the AppId will be null.  We generate a random AppId for this case only.
            // This is not required for production, since the AppId will have a value.
            if (string.IsNullOrEmpty(_appId))
            {
                _appId = Guid.NewGuid().ToString(); //if no AppId, use a random Guid
            }

            this.service = firebaseClient;
        }

        [HttpGet]
        public async Task Get(string batonName)
        {
            var batons = service.GetQueue().GetAwaiter().GetResult();

            var baton = batons.FirstOrDefault(x => x.Object.Name == batonName);
            var queue = baton?.Object.Queue;

            if (queue != null && queue.Count > 1)
            {
                var batonHolder = queue.Skip(1).FirstOrDefault();

                if (batonHolder != null)
                {
                    var attachments = new List<Attachment>();
                    var reply = MessageFactory.Attachment(attachments);
                    reply.Attachments.Add(
                        Card.GetUpdateYourBranchCard(batonName)
                            .ToAttachment());

                    await ((BotAdapter)_adapter).ContinueConversationAsync(
                        _appId,
                        batonHolder.Conversation,
                        async (context, token) =>
                            await BotCallback(reply, context, token),
                        default(CancellationToken));
                }
            }
        }
        private async Task BotCallback(IMessageActivity message, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl
            await turnContext.SendActivityAsync(message, cancellationToken);
        }
    }
}
