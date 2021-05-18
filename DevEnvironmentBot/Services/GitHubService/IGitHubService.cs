namespace BatonBot.GitHubService
{
    using System.Threading.Tasks;
    using BatonBot.Models;

    public interface IGitHubService
    {
         Task<ServiceResult> UpdatePullRequest(string repo, int prNumber);
         Task<ServiceResult> MergePullRequest(string repo, int issueNumber);
         Task<bool> CloseTicket(string repo, int issueNumber);
         PullRequest GetPRInfo(string repo, int prNumber);
         int GetTicketId(string repo, int prNumber);
    }
}
