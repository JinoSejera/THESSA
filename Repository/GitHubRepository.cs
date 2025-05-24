using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using THESSA.Contract;
using THESSA.Models;

namespace THESSA.Repository
{
    public class GitHubRepository : IGitHub
    {
        private readonly HttpClient _httpClient;

        public GitHubRepository(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentException(nameof(httpClient));
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("THESSA-App"); // GitHub requires a User-Agent
        }

        public async Task<string?> PostCommentToLineAsync(string token, PostCommentToLineRequestBody requestBody, RepositoryMetadata repoMetadata)
        {
            SetGitHubHeaders(token);
            string url = GetAddCommentURL(repoMetadata.RepositoryOwner!, repoMetadata.RepositoryName!, repoMetadata.PullRequestNumber);
            return await PostJsonAsync(url, requestBody, "line comment");
        }

        public async Task<string?> PostGeneralCommentAsync(string text, string token, RepositoryMetadata repoMetaData)
        {
            SetGitHubHeaders(token);
            var body = new { body = text };
            string url = GetAddIssueURL(repoMetaData.RepositoryOwner!, repoMetaData.RepositoryName!, repoMetaData.PullRequestNumber);
            return await PostJsonAsync(url, body, "general comment");
        }

        private void SetGitHubHeaders(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (!_httpClient.DefaultRequestHeaders.Accept.Contains(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json")))
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            }
        }

        private async Task<string?> PostJsonAsync(string url, object body, string errorContext)
        {
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new RepositoryException($"Error with {errorContext} {response.StatusCode} : {await response.Content.ReadAsStringAsync()}");
            }
        }

        private string GetAddCommentURL(string repoOwner, string repoName, int pullNumber)
        {
            return $"https://api.github.com/repos/{repoOwner}/{repoName}/pulls/{pullNumber}/comments";
        }

        private string GetAddIssueURL(string repoOwner, string repoName, int pullNumber)
        {
            return $"https://api.github.com/repos/{repoOwner}/{repoName}/issues/{pullNumber}/comments";
        }
    }
}
