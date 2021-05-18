namespace BatonBot.CommandHandlers
{
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using BatonBot.Interfaces;

    public class TokenCommandHandler : ITokenCommandHandler
    {
        private readonly BotState userState;

        public TokenCommandHandler(UserState userState)
        {
            this.userState = userState;
        }

        public async System.Threading.Tasks.Task SetHandler(string token, string name, ITurnContext<IMessageActivity> turnContext, System.Threading.CancellationToken cancellationToken)
        {
            var userStateAccessors = userState.CreateProperty<Models.UserProfile>(nameof(Models.UserProfile));
            var userProfile = await userStateAccessors.GetAsync(turnContext, () => new Models.UserProfile());
            userProfile.Token = token;
            userProfile.Name = name;
        }

        public async System.Threading.Tasks.Task ShowHandler(ITurnContext<IMessageActivity> turnContext, System.Threading.CancellationToken cancellationToken)
        {
            var userStateAccessors = userState.CreateProperty<Models.UserProfile>(nameof(Models.UserProfile));
            var userProfile = await userStateAccessors.GetAsync(turnContext, () => new Models.UserProfile());
            this.SendTokenValue(userProfile.Token, turnContext, cancellationToken);
        }

        private void SendTokenValue(string token, ITurnContext<IMessageActivity> turnContext, System.Threading.CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"Your token is set as : {token}");
            turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
