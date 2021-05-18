namespace BatonBot.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    public interface IGithubUpdateHandler
    {
        Task Handler(string type, int prNumber, string appId, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken);
    }
}
