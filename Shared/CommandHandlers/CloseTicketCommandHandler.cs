namespace SharedBaton.CommandHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using SharedBaton.GitHubService;
    using SharedBaton.Interfaces;

    public class CloseTicketCommandHandler : ICloseTicketCommandHandler
    {
        private readonly IGitHubService service;

        public CloseTicketCommandHandler(IGitHubService service)
        {
            this.service = service;
        }

        public async Task Handler(string type, int prNumber, string appId, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var repo = GetRepo(type);
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

        private string GetRepo(string batonName)
        {
            var repo = batonName switch
            {
                "be" => "maraschino",
                "fe" => "ADA-Research-UI",
                "man" => "ADA-Research-Configuration",
                _ => string.Empty
            };

            return repo;
        }
    }
}
