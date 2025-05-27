using THESSA.Models;

namespace THESSA.Contract
{
    public interface IGitHub
    {
        Task <string?> PostCommentToLineAsync(string token, PostCommentToLineRequestBody requestBody, RepositoryMetadata repoMetadata);
        Task <string?> PostGeneralCommentAsync(string text, string token, RepositoryMetadata repoMetaData);
        Task<List<FileData>> GetDiffFilesAsync(string token, RepositoryMetadata repoMetadata);
        Task<string> GetFileContentAsync(string token, string fileUrl);
    }
}
