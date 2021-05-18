
namespace BatonBot.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Models;

    public interface IWithinReleaseService
    {
        Task GotBaton(BatonRequest baton, string appId, bool notify, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken);
    }
}
