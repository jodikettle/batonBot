namespace SharedBaton.CommandHandlers
{
    using System.Linq;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using SharedBaton.Interfaces;
    using System.Threading;
    using System.Threading.Tasks;
    using SharedBaton.Firebase;

    public class TryAgainCommandHandler : ITryAgainCommandHandler
    {
        private readonly IFirebaseService service;
        private readonly IWithinReleaseService releaseService;

        public TryAgainCommandHandler(IFirebaseService firebaseService, IWithinReleaseService releaseService)
        {
            this.service = firebaseService;
            this.releaseService = releaseService;
        }

        public async Task Handler(string type, string appId, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var batons = service.GetQueue().GetAwaiter().GetResult();
            var batonFireObject = batons?.FirstOrDefault(x => x.Object.Name.Equals(type));

            if (batonFireObject == null) return;

            var queue = batonFireObject.Object.Queue;

            var name = turnContext.Activity.From.Name.Replace(" | Redington", "").Replace(" | Godel", "");

            if (queue.Count <= 0) return;

            // Does the first one belong to that person
            if (queue.First().UserName.Equals(name))
            {
                var baton = queue.FirstOrDefault();
                await this.releaseService.GotBaton(baton, string.Empty, false, turnContext, cancellationToken);
            }
            else
            {
                var reply = MessageFactory.Text($"Gotta wait your turn");
                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
        }
    }
}
