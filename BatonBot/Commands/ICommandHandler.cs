using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace BatonBot.Commands
{
    public interface ICommandHandler
    {
        void Handle(string text, string appId, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken);
    }
}
