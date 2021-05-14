
using Microsoft.AspNetCore.Mvc;

namespace DevEnvironmentBot.Controllers
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Configuration;
    using SharedBaton.Firebase;

    [Route("api/[controller]")]
    [ApiController]
    public class MasterBuildController : ControllerBase
    {
        private readonly IFirebaseService service;
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly int _timerCheckAllowance;

        public MasterBuildController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, IFirebaseService firebaseClient, ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _adapter = adapter;
            _conversationReferences = conversationReferences;
            _appId = configuration["MicrosoftAppId"];

            // If the channel is the Emulator, and authentication is not in use,
            // the AppId will be null.  We generate a random AppId for this case only.
            // This is not required for production, since the AppId will have a value.
            if (string.IsNullOrEmpty(_appId))
            {
                _appId = Guid.NewGuid().ToString(); //if no AppId, use a random Guid
            }

            this.service = firebaseClient;
        }

        [HttpGet("{batonName}")]
        public async Task Get(string batonName)
        {
            var batons = service.GetQueue().GetAwaiter().GetResult();

            var baton = batons.FirstOrDefault(x => x.Object.Name == batonName);
            var queue = baton?.Object.Queue;

            var batonHolder = queue?.FirstOrDefault();

            if (batonHolder != null)
            {
                var repoName = this.getRepoName(batonName);

                if (!string.IsNullOrEmpty(repoName))
                {
                    await ((BotAdapter)this._adapter).ContinueConversationAsync(
                        this._appId,
                        batonHolder.Conversation,
                        async (context, token) =>
                            await this.BotCallback( $"Build was successful time to deploy for {batonName}", context, token),
                        default(CancellationToken));
                }
            }
        }

        private string getRepoName(string type)
        {
            if (type == "be")
            {
                return "maraschino";
            }

            if (type == "fe")
            {
                return "ADA-Research-UI";
            }

            if (type == "man")
            {
                return "ADA-Research-Configuration";
            }

            return null;
        }

        private async Task BotCallback(string message, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl
            await turnContext.SendActivityAsync(message, cancellationToken: cancellationToken);
        }
    }
}
