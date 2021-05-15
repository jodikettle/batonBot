namespace SharedBaton.CommandHandlers
{
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using SharedBaton.Interfaces;
    using System.Threading;
    using System.Threading.Tasks;
    using SharedBaton.Models;

    public class TokenCommandHandler : ITokenCommandHandler
    {
        private BotState _userState;

        public TokenCommandHandler(UserState userState)
        {
            this._userState = userState;
        }

        public async Task SetHandler(string token, string name, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());
            userProfile.Token = token;
            userProfile.Name = name;
        }

        public async Task ShowHandler(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());
            this.SendTokenValue(userProfile.Token, turnContext, cancellationToken);
        }

        private void SendTokenValue(string token, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"Your token is set as : {token}");
            turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
