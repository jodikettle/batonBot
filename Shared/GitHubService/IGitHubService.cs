namespace SharedBaton.GitHubService
{
    using System.Threading.Tasks;

    public interface IGitHubService
    {
        public Task<bool> UpdatePullRequest(string repo, int prNumber);

        public Task CloseTicket(string repo, int prNumber);
    }
}
