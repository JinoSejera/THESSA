using System.Text.Json;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using THESSA.Contract;
using THESSA.Helper;
using THESSA.Models;
using THESSA.Repository;

namespace THESSA.Controllers
{

    [Route("api/v{version:apiVersion}/github/{repositoryOwner}/{repositoryName}/pull/{pullRequestNumber}/review")]
    [ApiController]
    [ApiVersion("1.0")]
    public class GitHubController : ControllerBase
    {
        private readonly ILogger<GitHubController> _logger;
        private readonly IGithubService _gitHubService;
        private readonly IThessaService _thessaService;
        public GitHubController(
            ILogger<GitHubController> logger,
            IGithubService gitHubService,
            IThessaService thessaService
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _gitHubService = gitHubService ?? throw new ArgumentNullException(nameof(gitHubService));
            _thessaService = thessaService ?? throw new ArgumentNullException(nameof(thessaService));

        }
        [HttpPost]
        public async Task<IActionResult> PostReview(
            [FromRoute] string repositoryOwner,
            [FromRoute] string repositoryName,
            [FromRoute] int pullRequestNumber,
            [FromHeader] GitHubMetadata metadata
            )
        {
            if (metadata == null)
            {
                return BadRequest("Header cannot be null");
            }
            try
            {
                string[] extensions;
                if (!string.IsNullOrEmpty(metadata.TargetExtensions))
                    extensions = metadata.TargetExtensions
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(ext => ext.Trim())
                        .ToArray();
                else extensions = [".cs", ".py"]; // Default file extensions

                //     // Process the data here
                //     // For example, you can log it or perform some action

                var repositoryRequestMetaData = new RepositoryMetadata
                {
                    RepositoryOwner = repositoryOwner,
                    RepositoryName = repositoryName,
                    PullRequestNumber = pullRequestNumber
                };

                var changedFiles = await _gitHubService.GetFilesDiffAsync(metadata.Token, repositoryRequestMetaData);

                // Replace the foreach loop with parallel async processing
                var contentTasks = changedFiles.Select(async file =>
                {
                    var content = await _gitHubService.GetFileContentAsync(metadata.Token, file.ContentUrl!);
                    var decodedContent = Utils.DecodeContentBase64(content);
                    file.Content = decodedContent;
                }).ToList();

                await Task.WhenAll(contentTasks);

                foreach (var file in changedFiles)
                {
                    // Checking if file extension of file to review is supported
                    var fileExtension = Path.GetExtension(file.FileName);
                    if (!extensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning($"Skipping, unsupported extension: {fileExtension} file: {file.FileName}");
                        continue;
                    }

                    // Checking if File content is empty
                    if (file.Content!.Length == 0)
                    {
                        _logger.LogWarning($"Skipping, empty content for file: {file.FileName}");
                        continue;
                    }

                    // Check if File Diff is Empty
                    if (file.Patch!.Length == 0)
                        _logger.LogWarning($"diff for file: {file.FileName} is Empty");

                    string separator = "\n\n----------------------------------------------------------------------\n\n";
                    _logger.LogInformation($"Asking Review for THESSA AI... Content Length: {file.Content.Length} Diff Length: {file.Patch.Length}.");

                    var thessaReview = await _thessaService.ReviewAsync(file.Content, file.Patch);

                    _logger.LogInformation($"{separator}{file.Content}{separator}{file.Patch}{separator}{thessaReview}{separator}");

                    if (_thessaService.IsNoIssueText(thessaReview))
                        _logger.LogInformation($"File: {file.FileName} looks good. Continue.");
                    else
                    {
                        var lineComments = _thessaService.SplitReview(thessaReview);

                        if (lineComments.Count == 0)
                        {
                            _logger.LogWarning($"No comments found for file: {file.FileName}");
                        }

                        bool reviewResult = false;
                        foreach (var lineComment in lineComments)
                        {
                            try
                            {
                                if (lineComment.Line != null)
                                    reviewResult = await _gitHubService.PostCommentToLineAsync(
                                        metadata.Token,
                                        lineComment.Comment!,
                                        file.CommitId!,
                                        file.FileName!,
                                        lineComment.Line,
                                        repositoryRequestMetaData);
                                if (!reviewResult)
                                    reviewResult = await _gitHubService.PostGeneralCommentAsync(lineComment.Comment!, metadata.Token, repositoryRequestMetaData);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error posting comment for file: {file.FileName} at line: {lineComment.Line}");
                                throw new RepositoryException("Failed to post any Review.", ex);
                            }
                        }
                    }
                }

                return Ok("Succed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GitHub request");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the request.");
            }
        }
    }
}
