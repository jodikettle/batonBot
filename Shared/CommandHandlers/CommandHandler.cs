﻿using System.Threading;
using System.Threading.Tasks;
using SharedBaton.Firebase;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using SharedBaton.Card;
using SharedBaton.Commands;
using SharedBaton.Interfaces;

namespace SharedBaton.CommandHandlers
{
    public class CommandHandler : ICommandHandler
    {
        private IBatonService batons;

        private readonly ITakeCommandHandler takeHandler;
        private readonly IReleaseCommandHandler releaseHandler;
        private readonly IAdminReleaseCommandHandler adminReleaseHandler;
        private readonly IShowCommandHandler showHandler;
        private readonly IConfiguration config;

        public CommandHandler(IFirebaseService client, ICardCreator cardCreator, IBatonService batonService,
            ITakeCommandHandler takeCommandHandler, IReleaseCommandHandler releaseCommandHandler, IAdminReleaseCommandHandler adminReleaseCommandHandler, IShowCommandHandler showCommandHandler, IConfiguration config)
        {
            this.batons = batonService;
            this.takeHandler = takeCommandHandler;
            this.releaseHandler = releaseCommandHandler;
            this.showHandler = showCommandHandler;
            this.adminReleaseHandler = adminReleaseCommandHandler;
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
                var name = text.Replace(command + " ", "").Replace(type + " ", "");

                if (string.IsNullOrEmpty(name))
                {
                    var activity = MessageFactory.Text($"You need to include a name");
                    _ = turnContext.SendActivityAsync(activity, cancellationToken);
                    return;
                }

                await this.adminReleaseHandler.Handler(batonType.Shortname, name, appId, turnContext, cancellationToken);
            }
            else if (command.Equals("release"))
            {
                await this.releaseHandler.Handler(batonType.Shortname, appId, turnContext, cancellationToken);
            }
            else if (command.Equals("take"))
            {
                var comment = text.Replace(command + " ", "").Replace(type, "");
                await this.takeHandler.Handler(batonType.Shortname, comment.TrimStart(), turnContext, cancellationToken);
            }
            else
            {
                var activity = MessageFactory.Text($"Huuuhhh?");
                _ = turnContext.SendActivityAsync(activity, cancellationToken);
            }
        }
    }
}
