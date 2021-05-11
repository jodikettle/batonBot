﻿

namespace SharedBaton.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Configuration;
    using SharedBaton.Firebase;
    using SharedBaton.Interfaces;
    using SharedBaton.Models;

    public class AdminReleaseCommandHandler : IAdminReleaseCommandHandler
    {
        private readonly IFirebaseService service;
        private readonly string releaseMessageText;
        private readonly IConfiguration config;
        private readonly IFirebaseLogger logger;

        public AdminReleaseCommandHandler(IFirebaseService firebaseClient, IConfiguration config, IFirebaseLogger logger)
        {
            this.service = firebaseClient;
            this.releaseMessageText = config["ReleaseBatonText"];
            this.logger = logger;
            this.config = config;
        }

        public async Task Handler(string type, string nameToRemove, string appId, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var batons = service.GetQueue().GetAwaiter().GetResult();
            var batonFireObject = batons?.FirstOrDefault(x => x.Object.Name.Equals(type));

            if (batonFireObject == null) return;

            var queue = batonFireObject.Object.Queue;

            if (queue.Count <= 0) return;

            // Does the first one belong to that person
            if (queue.First().UserName.ToLower().Equals(nameToRemove.ToLower()))
            {
                var oldBatonRequest = queue.Dequeue();

                if (queue.Count > 0)
                {
                    //Tell the other person
                    await this.Notify(queue.FirstOrDefault(), appId, turnContext);
                    queue.FirstOrDefault().DateReceived = DateTime.Now;
                }

                await service.UpdateQueue(batonFireObject);

                this.logger.Log(config["AppDisplayName"],
                    type, oldBatonRequest.UserName, oldBatonRequest.DateRequested, oldBatonRequest.DateReceived, DateTime.Now,
                    oldBatonRequest.MoveMeCount, true);

                var activity = MessageFactory.Text(this.releaseMessageText.Replace("{type}", type));
                await turnContext.SendActivityAsync(activity, cancellationToken);
            }
            else
            {
                var queueLength = queue.Count;
                batonFireObject.Object.Queue = this.removeAnyInQueue(queue, nameToRemove, turnContext, cancellationToken);

                if (batonFireObject.Object.Queue.Count() < queueLength)
                {
                    await service.UpdateQueue(batonFireObject);

                    var activity = MessageFactory.Text($"I have removed you from the queue");
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }
                else
                {
                    await this.SendNotYourBaton(turnContext, cancellationToken);
                }
                return;
            }
        }

        private Queue<BatonRequest> removeAnyInQueue(Queue<BatonRequest> batonQueue, string username, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return new Queue<BatonRequest>(batonQueue.Where(x => !x.UserName.ToLower().Equals(username.ToLower())));
        }

        private async Task Notify(BatonRequest batonRequest, string appId, ITurnContext<IMessageActivity> turnContext)
        {
            if (batonRequest == null || string.IsNullOrEmpty(batonRequest.UserId))
            {
                return;
            }

            if (batonRequest.Conversation != null)
            {
                await ((BotAdapter)turnContext.Adapter).ContinueConversationAsync(appId, batonRequest.Conversation, async (context, token) =>
                    await BotCallback(batonRequest.BatonName, context, token), default(CancellationToken));
            }
        }

        private async Task BotCallback(string name, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl
            await turnContext.SendActivityAsync($"Hey! its your turn with the {name} baton");
        }

        private async Task SendNotYourBaton(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"I couldn't find you in the list");
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}