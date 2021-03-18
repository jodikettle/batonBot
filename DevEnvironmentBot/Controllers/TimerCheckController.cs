using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using SharedBaton.Firebase;

namespace DevEnvironmentBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimerCheckController : ControllerBase
    {
        private readonly IFirebaseService service;
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly int _timerCheckAllowance;

        public TimerCheckController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, IFirebaseService firebaseClient, ConcurrentDictionary<string, ConversationReference> conversationReferences)
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
        public async Task Get()
        {
            var batons = service.GetQueue().GetAwaiter().GetResult();

            foreach (var baton in batons)
            {
                var queue = baton.Object.Queue;
                if (queue.Count > 0)
                {
                    var batonHolder = queue.FirstOrDefault();
                    var onehoursAgo = DateTime.Now.AddHours(_timerCheckAllowance * -1);
                    if (batonHolder.DateReceived < onehoursAgo)
                    {
                        if (batonHolder.Conversation != null)
                        {
                            await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, batonHolder.Conversation,
                                async (context, token) => {
                                    await context.SendActivityAsync($"Hey! whatcha got there? Is it? Oh it is the {baton.Object.Name} baton");
                                }, default(CancellationToken));
                        }
                    }
                }
            }
        }
    }
}
