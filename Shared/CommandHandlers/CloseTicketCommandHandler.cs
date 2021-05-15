namespace SharedBaton.CommandHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using SharedBaton.GitHubService;
    using SharedBaton.Interfaces;
    using SharedBaton.RepositoryMapper;

    public class CloseTicketCommandHandler : ICloseTicketCommandHandler
    {
        private readonly IGitHubService service;
        private readonly IRepositoryMapper mapper;

        public CloseTicketCommandHandler(IGitHubService service, IRepositoryMapper mapper)
        {
            this.service = service;
            this.mapper = mapper;
        }

        public async Task Handler(string batonName, int prNumber, string appId, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var repo = mapper.GetRepositoryNameFromBatonName(batonName);
            var result = await this.service.CloseTicket(repo, prNumber);

            if (result)
            {
                var reply = MessageFactory.Text($"All done");
                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
            else
            {
                var reply = MessageFactory.Text($"Sorry something went wrong - can you do it manually ");
                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
        }
    }
}
