namespace SharedBaton.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    public interface IMoveMeCommandHandler
    {
        Task Handler(string type, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken);
    }
}
