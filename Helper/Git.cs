using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using THESSA.Contract;

namespace THESSA.Helper
{
    public class Git : IGit
    {
        private readonly ILogger<Git> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _owner;
        private readonly string _repo;

        public Git(ILogger<Git> logger, HttpClient httpClient, string owner, string repo, string githubToken)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _owner = owner;
            _repo = repo;
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("THESSA", "1.0"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", githubToken);
        }

        public string GetRemoteName()
        {
            // Not directly available via GitHub API; return repo name as remote name
            return _repo;
        }

        public string GetLastCommitSha(string file)
        {
            var url = $"https://api.github.com/repos/{_owner}/{_repo}/commits?path={Uri.EscapeDataString(file)}&per_page=1";
            var response = _httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
            using var doc = JsonDocument.Parse(json);
            var sha = doc.RootElement[0].GetProperty("sha").GetString();
            return sha!;
        }

        public List<string> GetDiffFiles(string remoteName, string headRef, string baseRef)
        {
            var url = $"https://api.github.com/repos/{_owner}/{_repo}/compare/{baseRef}...{headRef}";
            var response = _httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
            using var doc = JsonDocument.Parse(json);
            var files = doc.RootElement.GetProperty("files");
            var result = new List<string>();
            foreach (var file in files.EnumerateArray())
            {
                result.Add(file.GetProperty("filename").GetString()!);
            }
            return result;
        }

        public string GetDiffInFile(string remoteName, string headRef, string baseRef, string filePath)
        {
            var url = $"https://api.github.com/repos/{_owner}/{_repo}/compare/{baseRef}...{headRef}";
            var response = _httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
            using var doc = JsonDocument.Parse(json);
            var files = doc.RootElement.GetProperty("files");
            foreach (var file in files.EnumerateArray())
            {
                if (file.GetProperty("filename").GetString() == filePath)
                {
                    return file.GetProperty("patch").GetString() ?? "";
                }
            }
            return "";
        }
    }

}
