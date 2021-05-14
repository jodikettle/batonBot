namespace SharedBaton.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    public interface IToughDayCommandHandler
    {
        Task Handler(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken);
    }
}
