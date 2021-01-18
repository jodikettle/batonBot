using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace SharedBaton.Interfaces
{
    public interface ICommandHandler
    {
        Task Handle(string text, string appId, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken);
    }
}
