using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace BatonBot.Commands
{
    public interface ITakeCommandHandler
    {
        void Handler(string type, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken);
    }
}
