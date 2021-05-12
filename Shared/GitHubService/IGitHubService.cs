namespace SharedBaton.GitHubService
{
    using System.Threading.Tasks;
    using SharedBaton.Models;

    public interface IGitHubService
    {
         Task<ServiceResult> UpdatePullRequest(string repo, int prNumber);
         Task<bool> MergePullRequest(string repo, int issueNumber);
         Task<bool> CloseTicket(string repo, int issueNumber);
         Task<PullRequest> getPRInfo(string repo, int prNumber);
    }
}
