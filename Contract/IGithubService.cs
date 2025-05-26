using THESSA.Models;

namespace THESSA.Contract
{
    public interface IGithubService
    {
        Task<bool> PostCommentToLineAsync(string token, string message, string commitId, string filePath, int position, RepositoryMetadata repoMetadata);
        Task<bool> PostGeneralCommentAsync(string text, string token, RepositoryMetadata repoMetaData);
        Task<List<FileData>> GetFilesDiffAsync(string token, RepositoryMetadata repoMetadata);
        Task<string> GetFileContentAsync(string token, string fileUrl);


    }
}
