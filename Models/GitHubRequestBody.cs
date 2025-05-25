using System.ComponentModel.DataAnnotations;

namespace THESSA.Models
{
    public class GitHubRequestBody
    {
        [Required]
        public string RepositoryName { get; set; }

        [Required]
        public int PullRequestNumber { get; set; }

        [Required]
        public string RepositoryOwner { get; set; }

        [Required]
        public string CommitId { get; set; }
    }
}
