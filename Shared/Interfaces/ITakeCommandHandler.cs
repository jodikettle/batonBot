using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace SharedBaton.Commands
{
    public interface ITakeCommandHandler
    {
        void Handler(string type, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken);
    }
}
