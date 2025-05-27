using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using THESSA.Contract;
using THESSA.Models;

namespace THESSA.Repository
{
    /// <summary>
    /// Repository for interacting with GitHub API for pull request comments and file diffs.
    /// </summary>
    public class GitHubRepository : IGitHub
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubRepository"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for requests.</param>
        public GitHubRepository(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentException(nameof(httpClient));
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("THESSA-App"); // GitHub requires a User-Agent
        }

        /// <summary>
        /// Posts a comment to a specific line in a pull request.
        /// </summary>
        /// <param name="token">GitHub access token.</param>
        /// <param name="requestBody">Request body containing comment details.</param>
        /// <param name="repoMetadata">Repository metadata.</param>
        /// <returns>Response content as string.</returns>
        public async Task<string?> PostCommentToLineAsync(string token, PostCommentToLineRequestBody requestBody, RepositoryMetadata repoMetadata)
        {
            SetGitHubHeaders(token);
            string url = GetAddCommentURL(repoMetadata.RepositoryOwner!, repoMetadata.RepositoryName!, repoMetadata.PullRequestNumber);
            var response = await PostJsonAsync(url, requestBody, "line comment");

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Posts a general comment to a pull request.
        /// </summary>
        /// <param name="text">Comment text.</param>
        /// <param name="token">GitHub access token.</param>
        /// <param name="repoMetaData">Repository metadata.</param>
        /// <returns>Response content as string.</returns>
        public async Task<string?> PostGeneralCommentAsync(string text, string token, RepositoryMetadata repoMetaData)
        {
            SetGitHubHeaders(token);
            var body = new { body = text };
            string url = GetAddIssueURL(repoMetaData.RepositoryOwner!, repoMetaData.RepositoryName!, repoMetaData.PullRequestNumber);
            var response = await PostJsonAsync(url, body, "general comment");

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Gets the list of files changed in a pull request along with their diff patches.
        /// </summary>
        /// <param name="token">GitHub access token.</param>
        /// <param name="repoMetadata">Repository metadata.</param>
        /// <returns>List of file data with diffs.</returns>
        public async Task<List<FileData>> GetDiffFilesAsync(string token, RepositoryMetadata repoMetadata)
        {
            SetGitHubHeaders(token);
            string url = GetFileListURL(repoMetadata.RepositoryOwner!, repoMetadata.RepositoryName!, repoMetadata.PullRequestNumber);
            var response = await GetJsonAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            var result = new List<FileData>();
            using (var doc = JsonDocument.Parse(json))
            {
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (element.TryGetProperty("contents_url", out var contentsUrlProp) && element.TryGetProperty("filename", out var filenameProp) && element.TryGetProperty("patch", out var patchProp))
                    {
                        var fileData = new FileData();
                        var contentsUrl = contentsUrlProp.GetString();
                        var filename = filenameProp.GetString();
                        var patch = patchProp.GetString();
                        if (!string.IsNullOrEmpty(contentsUrl))
                        {
                            fileData.ContentUrl = contentsUrl;

                            Uri contentUri = new Uri(contentsUrl);
                            string query = contentUri.Query;

                            var queryParams = HttpUtility.ParseQueryString(query);
                            fileData.CommitId = queryParams["ref"];
                        }
                        if (!string.IsNullOrEmpty(filename))
                        {
                            fileData.FileName = filename;
                        }
                        if (!string.IsNullOrEmpty(patch))
                        {
                            fileData.Patch = patch;
                        }
                        result.Add(fileData);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the content of a file from GitHub.
        /// </summary>
        /// <param name="token">GitHub access token.</param>
        /// <param name="fileUrl">URL to the file content API endpoint.</param>
        /// <returns>File content as a string.</returns>
        public async Task<string> GetFileContentAsync(string token, string fileUrl)
        {
            SetGitHubHeaders(token);
            var response = await GetJsonAsync(fileUrl);
            var json = await response.Content.ReadAsStringAsync();

            using(var doc = JsonDocument.Parse(json))
            {
                if (doc.RootElement.TryGetProperty("content", out var contentProp))
                {
                    return contentProp.GetString()!;
                }
                else
                {
                    throw new RepositoryException("Invalid response format: 'content' not found");
                }
            }

        }

        private void SetGitHubHeaders(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (!_httpClient.DefaultRequestHeaders.Accept.Contains(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json")))
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            }
        }

        private async Task<HttpResponseMessage> PostJsonAsync(string url, object body, string errorContext)
        {
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                return response;
            }
            else
            {
                throw new RepositoryException($"Error with {errorContext} {response.StatusCode} : {await response.Content.ReadAsStringAsync()}");
            }
        }

        private async Task<HttpResponseMessage> GetJsonAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return response;
            }
            else
            {
                throw new RepositoryException($"Error fetching data from {url} - Status Code: {response.StatusCode}");
            }

        }

        private string GetFileListURL(string repoOwner, string repoName, int pullNumber)
        {
            return $"https://api.github.com/repos/{repoOwner}/{repoName}/pulls/{pullNumber}/files";
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
