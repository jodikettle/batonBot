﻿
namespace SharedBaton.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using SharedBaton.Models;

    public interface IWithinReleaseService
    {
        Task GotBaton(BatonRequest baton, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken);
    }
}
