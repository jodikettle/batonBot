using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace SharedBaton.Interfaces
{
    public interface IShowCommandHandler
    {
        void Handler(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken);
    }
}
