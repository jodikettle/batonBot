namespace DevEnvironmentBot.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Configuration;
    using SharedBaton.Card;
    using SharedBaton.Firebase;
    using SharedBaton.RepositoryMapper;

    [Route("api/[controller]")]
    [ApiController]
    public class SomeonesMergedController : ControllerBase
    {
        private readonly IFirebaseService service;
        private readonly IRepositoryMapper repoMapper;
        private readonly ICardCreator cardCreator;
        private readonly IBotFrameworkHttpAdapter adapter;
        private readonly string appId;

        public SomeonesMergedController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, IFirebaseService firebaseClient, 
            ICardCreator cardCreator, IRepositoryMapper repoMapper)
        {
            this.service = firebaseClient;
            this.adapter = adapter;
            this.appId = configuration["MicrosoftAppId"];
            this.cardCreator = cardCreator;
            this.repoMapper = repoMapper;

            if (string.IsNullOrEmpty(appId))
            {
                this.appId = Guid.NewGuid().ToString(); //if no AppId, use a random Guid
            }
        }

        [HttpGet("{batonName}")]
        public async Task Get(string batonName)
        {
            var queue = await service.GetQueueForBaton(batonName);

            if (queue != null && queue.Count > 1)
            {
                var batonHolder = queue.Skip(1).FirstOrDefault();

                if (batonHolder != null)
                {
                    var repoName = this.repoMapper.GetRepositoryNameFromBatonName(batonName);

                    if (!string.IsNullOrEmpty(repoName))
                    {
                        var reply = MessageFactory.Attachment(new List<Attachment>());
                        reply.Attachments.Add(
                            this.cardCreator.GetUpdateYourBranchCard(batonName, repoName, batonHolder.PullRequestNumber)
                                .ToAttachment());

                        await ((BotAdapter)adapter).ContinueConversationAsync(
                            appId,
                            batonHolder.Conversation,
                            async (context, token) =>
                                await SendMessage(reply, context, token),
                            default(CancellationToken));
                    }
                }
            }
        }

        private async Task SendMessage(IMessageActivity message, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl
            await turnContext.SendActivityAsync(message, cancellationToken);
        }
    }
}
