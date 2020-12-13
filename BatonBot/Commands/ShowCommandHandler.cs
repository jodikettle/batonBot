﻿using System.Threading;
using SharedBaton.Firebase;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using SharedBaton.Services;
using SharedBaton.Card;

namespace BatonBot.Commands
{
    public class ShowCommandHandler
    {
        private readonly GetAndDisplayBatonService service;

        public ShowCommandHandler(IFirebaseService client, ICardCreator cardCreator)
        {
            this.service = new GetAndDisplayBatonService(client, cardCreator);
        }

        public async void Handler(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            await this.service.SendBatons(turnContext, cancellationToken);
        }
    }
}
