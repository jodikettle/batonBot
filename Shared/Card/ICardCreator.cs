using Firebase.Database;
using Microsoft.Bot.Schema;
using SharedBaton.Models;
using System.Collections.Generic;

namespace SharedBaton.Card
{
    using SharedBaton.GitHubService;

    public interface ICardCreator
    {
        Attachment CreateBatonsAttachment(IList<FirebaseObject<BatonQueue>> batons);
        HeroCard GetUpdateYourBranchCard(string batonName, string repoName, int prNumber);
        HeroCard GetUpdateYourBranchCardBeforeMerge(string batonName, string repoName, int prNumber);
        HeroCard SquashAndMergeCard(string batonName, string repoName, int prNumber);
        HeroCard DoYouWantToCloseTheTicket(string batonName, string repoName, int prNumber, IGitHubService service);
    }
}
