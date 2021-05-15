namespace SharedBaton.GitHubService
{
    public class ServiceResult
    {
        public bool Succeeded { get; set; }
        public string MergeStatus { get; set; }
        public string ReasonForFailure { get; set; }
    }
}
