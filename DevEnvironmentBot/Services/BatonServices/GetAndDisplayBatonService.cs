﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Database;
using Microsoft.Bot.Builder;
using BatonBot.Models;

namespace BatonBot.BatonServices
{
    using BatonBot.Firebase;
    using BatonBot.Services.Card;

    public class GetAndDisplayBatonService
    {
        private readonly IFirebaseService firebaseClient;
        private readonly ICardCreator cardCreator;

        public GetAndDisplayBatonService(IFirebaseService firebaseClient, ICardCreator cardCreator)
        {
            this.firebaseClient = firebaseClient;
            this.cardCreator = cardCreator;
        }

        public async Task SendBatons(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var batons = this.firebaseClient.GetQueues().GetAwaiter().GetResult();
            await SendBatonInfoAsync(turnContext, cancellationToken, batons);
        }

        private async Task SendBatonInfoAsync(ITurnContext turnContext,
            CancellationToken cancellationToken,
            IList<FirebaseObject<BatonQueue>> batons)
        {
            var card = this.cardCreator.CreateBatonsAttachment(batons);

            var reply = MessageFactory.Attachment(card);
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
