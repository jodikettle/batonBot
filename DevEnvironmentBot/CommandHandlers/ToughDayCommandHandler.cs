namespace BatonBot.CommandHandlers
{
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using System.Threading;
    using System.Threading.Tasks;
    using BatonBot.Interfaces;

    public class ToughDayCommandHandler : IToughDayCommandHandler
    {
        public async Task Handler(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"There there, it will be okay");
            _ = await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
