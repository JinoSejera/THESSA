namespace THESSA.Models
{
    public class GitHubRequestBody
    {
        public string? RepositoryName { get; set; }
        public int PullRequestNumber { get; set; }
        public string? RepositoryOwner { get; set; }
        public string? CommitId { get; set; }
    }
}
