using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;

namespace BatonBot.Controllers
{
    using BatonBot.Firebase;

    [Route("api/[controller]")]
    [ApiController]
    public class TimerCheckController : ControllerBase
    {
        private readonly IFirebaseService service;
        private readonly IBotFrameworkHttpAdapter adapter;
        private readonly string appId;
        private readonly int timerCheckAllowance;

        public TimerCheckController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, IFirebaseService firebaseClient)
        {
            this.adapter = adapter;
            this.appId = configuration["MicrosoftAppId"];
            this.timerCheckAllowance = Int32.Parse(configuration["TimerCheckAllowance"]);
            this.service = firebaseClient;

            if (string.IsNullOrEmpty(this.appId))
            {
                this.appId = Guid.NewGuid().ToString(); //if no AppId, use a random Guid
            }
        }

        [HttpGet]
        public async Task Get()
        {
            var batons = service.GetQueues().GetAwaiter().GetResult();

            foreach (var baton in batons)
            {
                var queue = baton.Object.Queue;
                if (queue.Count > 0)
                {
                    var batonHolder = queue.FirstOrDefault();
                    var hoursAgo = DateTime.Now.AddHours(timerCheckAllowance * -1);
                    if (batonHolder.DateReceived < hoursAgo)
                    {
                        if (batonHolder.Conversation != null)
                        {
                            await ((BotAdapter)adapter).ContinueConversationAsync(appId, batonHolder.Conversation,
                                async (context, token) =>
                                {
                                    await context.SendActivityAsync($"Hey! whatcha got there? Is it? Oh it is the {baton.Object.Name} baton");
                                }, default(CancellationToken));
                        }
                    }
                }
            }
        }
    }
}
