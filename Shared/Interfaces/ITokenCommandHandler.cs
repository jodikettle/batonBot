namespace SharedBaton.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    public interface ITokenCommandHandler
    {
        Task SetHandler(string tokenString, string name, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken);

        Task ShowHandler(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken);
    }
}
