using THESSA.Contract;
using THESSA.Models;

namespace THESSA.Services;

public class GitHubService : IGithubService
{
    private readonly IGitHub _gitHubRepository;
    private ILogger<GitHubService> _logger;

    public GitHubService(IGitHub gitHubRepository, ILogger<GitHubService> logger)
    {
        _gitHubRepository = gitHubRepository ?? throw new ArgumentNullException(nameof(gitHubRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    }
    public Task<string?> PostCommentToLineAsync(string token, PostCommentToLineRequestBody requestBody, RepositoryMetadata repoMetadata)
    {
        throw new NotImplementedException();
    }

    public Task<string?> PostGeneralCommentAsync(string text, string token, RepositoryMetadata repoMetaData)
    {
        throw new NotImplementedException();
    }
}
