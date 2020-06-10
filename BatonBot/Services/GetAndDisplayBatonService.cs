using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BatonBot.Firebase;
using BatonBot.Models;
using Firebase.Database;
using Microsoft.Bot.Builder;

namespace BatonBot.Services
{
    public class GetAndDisplayBatonService
    {
        private readonly IFirebaseClient firebaseClient;

        public GetAndDisplayBatonService(IFirebaseClient firebaseClient)
        {
            this.firebaseClient = firebaseClient;
        }

        public async Task SendBatons(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var batons = this.firebaseClient.GetQueue().GetAwaiter().GetResult();
            await SendBatonInfoAsync(turnContext, cancellationToken, batons);
        }

        private static async Task SendBatonInfoAsync(ITurnContext turnContext,
            CancellationToken cancellationToken,
            IList<FirebaseObject<BatonQueue>> batons)
        {
            var card = Cards.Card.CreateBatonsAttachment(batons);

            var reply = MessageFactory.Attachment(card);
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
