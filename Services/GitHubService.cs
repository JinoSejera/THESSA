using THESSA.Contract;
using THESSA.Models;
using THESSA.Helper;

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
    public async Task<bool> PostCommentToLineAsync(string token, string message, string commitId, string filePath, int position, RepositoryMetadata repoMetadata)
    {
        try
        {
            var requestBody = new PostCommentToLineRequestBody
            {
                body = message,
                commit_id = commitId,
                path = filePath,
                line = position

            };

            var response = await _gitHubRepository.PostCommentToLineAsync(token, requestBody, repoMetadata);
            _logger.LogInformation($"Review Posted: {response}");
            return true;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting comment to line");
            return false;
        }
    }

    public async Task<List<FileData>> GetFilesDiffAsync(string token, RepositoryMetadata repoMetadata)
    {
        try
        {
            return await _gitHubRepository.GetDiffFilesAsync(token, repoMetadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting files diff");
            throw;
        }
    }

    public async Task<string> GetFileContentAsync(string token, string fileUrl)
    {
        try
        {
            return await _gitHubRepository.GetFileContentAsync(token, fileUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file content");
            throw;
        }
    }

    public async Task<bool> PostGeneralCommentAsync(string text, string token, RepositoryMetadata repoMetaData)
    {
        try
        {
            var response = await _gitHubRepository.PostGeneralCommentAsync(text, token, repoMetaData);
            _logger.LogInformation($"Posted General Review: {response}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting general comment");
            throw;
        }
    }
}
