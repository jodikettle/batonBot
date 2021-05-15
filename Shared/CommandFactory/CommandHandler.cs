using System.Threading;
using System.Threading.Tasks;
using SharedBaton.Firebase;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using SharedBaton.Card;
using SharedBaton.Commands;
using SharedBaton.Interfaces;

namespace SharedBaton.CommandFactory
{
    public class CommandHandler : ICommandHandler
    {
        private IBatonService batons;

        private readonly ITakeCommandHandler takeHandler;
        private readonly IReleaseCommandHandler releaseHandler;
        private readonly IAdminReleaseCommandHandler adminReleaseHandler;
        private readonly IMoveMeCommandHandler moveMeHandler;
        private readonly IShowCommandHandler showHandler;
        private readonly IGithubUpdateHandler githubUpdateHandler;
        private readonly IGithubMergeHandler githubMergeHandler;
        private readonly ICloseTicketCommandHandler closeTicketHandler;
        private readonly ITryAgainCommandHandler tryAgainHandler;
        private readonly IToughDayCommandHandler toughDayCommandHandler;
        private readonly ITokenCommandHandler tokenHandler;
        private readonly IConfiguration config;

        public CommandHandler(IFirebaseService client, ICardCreator cardCreator, IBatonService batonService,
            ITakeCommandHandler takeCommandHandler, IReleaseCommandHandler releaseCommandHandler, IAdminReleaseCommandHandler adminReleaseCommandHandler, IShowCommandHandler showCommandHandler, IMoveMeCommandHandler moveMeCommandHandler, IGithubUpdateHandler githubUpdateHandler, IGithubMergeHandler githubMergeHandler,
            ICloseTicketCommandHandler closeTicketHandler, ITryAgainCommandHandler tryAgainHandler, IToughDayCommandHandler toughDayCommandHandler, ITokenCommandHandler tokenHandler, IConfiguration config)
        {
            this.batons = batonService;
            this.takeHandler = takeCommandHandler;
            this.releaseHandler = releaseCommandHandler;
            this.showHandler = showCommandHandler;
            this.adminReleaseHandler = adminReleaseCommandHandler;
            this.moveMeHandler = moveMeCommandHandler;
            this.githubUpdateHandler = githubUpdateHandler;
            this.githubMergeHandler = githubMergeHandler;
            this.closeTicketHandler = closeTicketHandler;
            this.tryAgainHandler = tryAgainHandler;
            this.toughDayCommandHandler = toughDayCommandHandler;
            this.tokenHandler = tokenHandler;
            this.config = config;
        }

        public async Task Handle(string text, string appId, ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            // Filter out mention 
            if (text.Contains("list"))
            {
                var activity = MessageFactory.Text($"Batons are {this.batons.List()}");
                _ = turnContext.SendActivityAsync(activity, cancellationToken);
                return;
            }
            if (text.Contains("show"))
            {
                this.showHandler.Handler(turnContext, cancellationToken);
                return;
            }
            if (text.Contains("tough day"))
            {
                await this.toughDayCommandHandler.Handler(turnContext, cancellationToken);
                return;
            }
            if (text.Contains("view token"))
            {
                await this.tokenHandler.ShowHandler(turnContext, cancellationToken);
                return;
            }
            if (text.Contains("set token"))
            {
                var comment = text.Replace( "set token ", "").Trim();
                await this.tokenHandler.SetHandler(comment, turnContext.Activity.From.Name, turnContext, cancellationToken);
                return;
            }
            if (text.IndexOf(' ') == -1)
            {
                var activity = MessageFactory.Text($"Sorry I don't quite understand that?");
                _ = turnContext.SendActivityAsync(activity, cancellationToken);
                return;
            }

            var command = text.Substring(0, text.IndexOf(' '));
            var type = text.Replace(command + " ", "");
            type = type.IndexOf(' ') > 0 ? type.Substring(0, type.IndexOf(' ')) : type;
            var batonType = batons.CheckBatonType(type);
            if (batonType == null)
            {
                var activity = MessageFactory.Text($"Baton {type} is not a thing. But these are {this.batons.List()}");
                _ = turnContext.SendActivityAsync(activity, cancellationToken);
                return;
            }

            if (command.Equals("bananapancake"))
            {
                var name = text.Replace(command + " ", "").Replace(type, "").TrimStart();

                if (string.IsNullOrEmpty(name))
                {
                    var activity = MessageFactory.Text($"You need to include a name");
                    _ = turnContext.SendActivityAsync(activity, cancellationToken);
                    return;
                }

                await this.adminReleaseHandler.Handler(batonType.Shortname, name, appId, turnContext, cancellationToken);
            }
            else if (command.Equals("moveme"))
            {
                await this.moveMeHandler.Handler(batonType.Shortname, appId, turnContext, cancellationToken);
            }
            else if (command.Equals("release"))
            {
                await this.releaseHandler.Handler(batonType.Shortname, appId, turnContext, cancellationToken);
            }
            else if (command.Equals("tryagain"))
            {
                await this.tryAgainHandler.Handler(batonType.Shortname, appId, turnContext, cancellationToken);
            }
            else if (command.Equals("take"))
            {
                var comment = text.Replace(command + " ", "").Replace(type, "").Trim();
                var tokenised = comment.Trim().Split(" ");

                int prNumber = 0;

                if (tokenised.Length > 0)
                {
                    int.TryParse(tokenised[0], out prNumber);
                }

                if (prNumber > 0)
                {
                    comment = comment.Replace(prNumber.ToString(), "").Trim();
                }

                await this.takeHandler.Handler(batonType.Shortname, comment.TrimStart(), prNumber, turnContext, cancellationToken);
            }
            else if (command.Equals("updategithub"))
            {
                var pr = text.Replace(command + " ", "").Replace(type, "").Trim();
                int.TryParse(pr, out var prNumber);

                await this.githubUpdateHandler.Handler(batonType.Shortname, prNumber, appId, turnContext, cancellationToken);
            }
            else if (command.Equals("mergegithub"))
            {
                var pr = text.Replace(command + " ", "").Replace(type, "").Trim();
                int.TryParse(pr, out var prNumber);

                await this.githubMergeHandler.Handler(batonType.Shortname, prNumber, appId, turnContext, cancellationToken);
            }
            else if (command.Equals("closeticket"))
            {
                var pr = text.Replace(command + " ", "").Replace(type, "").Trim();
                int.TryParse(pr, out var prNumber);

                await this.closeTicketHandler.Handler(batonType.Shortname, prNumber, appId, turnContext, cancellationToken);
            }
            else
            {
                var activity = MessageFactory.Text($"Huuuhhh?");
                _ = turnContext.SendActivityAsync(activity, cancellationToken);
            }
        }
    }
}
