
using Microsoft.AspNetCore.Mvc;

namespace DevEnvironmentBot.Controllers
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Extensions.Configuration;
    using SharedBaton.Firebase;
    using SharedBaton.RepositoryMapper;

    [Route("api/[controller]")]
    [ApiController]
    public class MasterBuildController : ControllerBase
    {
        private readonly IFirebaseService service;
        private readonly IBotFrameworkHttpAdapter adapter;
        private readonly IRepositoryMapper mapper;
        private readonly string appId;

        public MasterBuildController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, IFirebaseService firebaseClient, IRepositoryMapper mapper)
        {
            this.adapter = adapter;
            this.appId = configuration["MicrosoftAppId"];
            this.service = firebaseClient;
            this.mapper = mapper;

            if (string.IsNullOrEmpty(this.appId))
            {
                this.appId = Guid.NewGuid().ToString(); //if no AppId, use a random Guid
            }
        }

        [HttpGet("{batonName}")]
        public async Task Get(string batonName)
        {
            var queue = await service.GetQueueForBaton(batonName);

            var batonHolder = queue?.FirstOrDefault();

            if (batonHolder != null)
            {
                var repoName = this.mapper.GetRepositoryNameFromBatonName(batonName);

                if (!string.IsNullOrEmpty(repoName))
                {
                    await ((BotAdapter)this.adapter).ContinueConversationAsync(
                        this.appId,
                        batonHolder.Conversation,
                        async (context, token) =>
                            await this.SendMessage( $"Build was successful time to deploy for {batonName}", context, token),
                        default(CancellationToken));
                }
            }
        }

        private async Task SendMessage(string message, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(message, cancellationToken: cancellationToken);
        }
    }
}
