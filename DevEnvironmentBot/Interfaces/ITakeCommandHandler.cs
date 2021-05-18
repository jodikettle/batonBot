using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace BatonBot.Commands
{
    public interface ITakeCommandHandler
    {
        Task Handler(string type, string comment, int pullRequest, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken);
    }
}
