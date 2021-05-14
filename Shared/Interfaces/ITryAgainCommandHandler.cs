namespace SharedBaton.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    public interface ITryAgainCommandHandler
    {
        Task Handler(string type, string appId, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken);
    }
}
