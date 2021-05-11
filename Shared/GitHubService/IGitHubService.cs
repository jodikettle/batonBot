namespace SharedBaton.GitHubService
{
    public interface IGitHubService
    {
        public void UpdatePullRequest(string repo, string prNumber);
    }
}
