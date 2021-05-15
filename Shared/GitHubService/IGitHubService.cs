namespace SharedBaton.GitHubService
{
    using System.Threading.Tasks;
    using SharedBaton.Models;

    public interface IGitHubService
    {
         Task<ServiceResult> UpdatePullRequest(string repo, int prNumber);
         Task<ServiceResult> MergePullRequest(string repo, int issueNumber);
         Task<bool> CloseTicket(string repo, int issueNumber);
         PullRequest GetPRInfo(string repo, int prNumber);

         Task<int> GetTicketId(string repo, int prNumber);
    }
}
