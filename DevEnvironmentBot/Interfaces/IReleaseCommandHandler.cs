using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace BatonBot.Interfaces
{
    public interface IReleaseCommandHandler
    {
        Task Handler(string type, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken);

        Task AdminHandler(string type, string name, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken);

        Task MoveMeHandler(string type, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken);
    }
}
