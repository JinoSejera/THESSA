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

                //var changedFiles = await _gitHubService.GetFilesDiffAsync(metadata.Token, repositoryRequestMetaData);

                //// Replace the foreach loop with parallel async processing
                //var contentTasks = changedFiles.Select(async file =>
                //{
                //    var content = await _gitHubService.GetFileContentAsync(metadata.Token, file.ContentUrl!);
                //    var decodedContent = Utils.DecodeContentBase64(content);
                //    file.Content = decodedContent;
                //}).ToList();

                //await Task.WhenAll(contentTasks);

                ////foreach (var file in changedFiles)
                ////{
                ////    // Checking if file extension of file to review is supported
                ////    var fileExtension = Path.GetExtension(file.FileName);
                ////    if (!extensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
                ////    {
                ////        _logger.LogWarning($"Skipping, unsupported extension: {fileExtension} file: {file.FileName}");
                ////        continue;
                ////    }

                ////    // Checking if File content is empty
                ////    if (file.Content!.Length == 0)
                ////    {
                ////        _logger.LogWarning($"Skipping, empty content for file: {file.FileName}");
                ////        continue;
                ////    }

                ////    // Check if File Diff is Empty
                ////    if (file.Patch!.Length == 0)
                ////        _logger.LogWarning($"diff for file: {file.FileName} is Empty");

                ////    string separator = "\n\n----------------------------------------------------------------------\n\n";
                ////    _logger.LogInformation($"Asking Review for THESSA AI... Content Length: {file.Content.Length} Diff Length: {file.Patch.Length}.");

                ////    var thessaReview = await _thessaService.ReviewAsync(file.Content, file.Patch);

                ////    _logger.LogInformation($"{separator}{file.Content}{separator}{file.Patch}{separator}{thessaReview}{separator}");

                ////    if (_thessaService.IsNoIssueText(thessaReview))
                ////        _logger.LogInformation($"File: {file.FileName} looks good. Continue.");
                ////    else
                ////    {
                ////        var lineComments = _thessaService.SplitReview(thessaReview);

                ////        if (lineComments.Count == 0)
                ////        {
                ////            _logger.LogWarning($"No comments found for file: {file.FileName}");
                ////        }

                ////        bool reviewResult = false;
                ////        foreach (var lineComment in lineComments)
                ////        {
                ////            try
                ////            {
                ////                if (lineComment.Line != null)
                ////                    reviewResult = await _gitHubService.PostCommentToLineAsync(
                ////                        metadata.Token,
                ////                        lineComment.Comment!,
                ////                        file.CommitId!,
                ////                        file.FileName!,
                ////                        lineComment.Line,
                ////                        repositoryRequestMetaData);
                ////                if (!reviewResult)
                ////                    reviewResult = await _gitHubService.PostGeneralCommentAsync(lineComment.Comment!, metadata.Token, repositoryRequestMetaData);
                ////            }
                ////            catch (Exception ex)
                ////            {
                ////                _logger.LogError(ex, $"Error posting comment for file: {file.FileName} at line: {lineComment.Line}");
                ////                throw new RepositoryException("Failed to post any Review.", ex);
                ////            }
                ////        }
                ////    }
                ////}
                var response = await _thessaService.ReviewAsync(
                    diff: "@@ -1,8 +1,12 @@\n \ufeffusing System.Text.Json;\n+using System.Threading.Tasks;\n using Asp.Versioning;\n using Microsoft.AspNetCore.Http;\n using Microsoft.AspNetCore.Mvc;\n+using THESSA.Contract;\n+using THESSA.Helper;\n using THESSA.Models;\n+using THESSA.Repository;\n \n namespace THESSA.Controllers\n {\n@@ -13,25 +17,131 @@ namespace THESSA.Controllers\n     public class GitHubController : ControllerBase\n     {\n         private readonly ILogger<GitHubController> _logger;\n-        public GitHubController(ILogger<GitHubController> logger)\n+        private readonly IGithubService _gitHubService;\n+        private readonly IThessaService _thessaService;\n+        public GitHubController(\n+            ILogger<GitHubController> logger,\n+            IGithubService gitHubService,\n+            IThessaService thessaService\n+            )\n         {\n             _logger = logger ?? throw new ArgumentNullException(nameof(logger));\n+            _gitHubService = gitHubService ?? throw new ArgumentNullException(nameof(gitHubService));\n+            _thessaService = thessaService ?? throw new ArgumentNullException(nameof(thessaService));\n \n         }\n         [HttpPost]\n-        public IActionResult PostReview(\n+        public async Task<IActionResult> PostReview(\n             [FromHeader] GitHubMetadata metadata,\n             [FromBody] GitHubRequestBody body)\n         {\n             if (metadata == null || body == null)\n             {\n                 return BadRequest(\"Metadata and body cannot be null\");\n             }\n-            _logger.LogInformation(metadata.ToString());\n-            _logger.LogInformation($\"Received GitHub request with metadata: {JsonSerializer.Serialize(metadata)} and body: {JsonSerializer.Serialize(body)}\");\n-            // Process the data here\n-            // For example, you can log it or perform some action\n-            return Ok(\"Data received successfully\");\n+            try\n+            {\n+                string[] extensions;\n+                if (!string.IsNullOrEmpty(metadata.TargetExtensions))\n+                    extensions = metadata.TargetExtensions\n+                        .Split(',', StringSplitOptions.RemoveEmptyEntries)\n+                        .Select(ext => ext.Trim())\n+                        .ToArray();\n+                else extensions = [\".cs\", \".py\"]; // Default file extensions\n+\n+                _logger.LogInformation($\"Received GitHub request with metadata: {JsonSerializer.Serialize(metadata)} and body: {JsonSerializer.Serialize(body)}\");\n+                // Process the data here\n+                // For example, you can log it or perform some action\n+\n+                var repositoryRequestMetaData = new RepositoryMetadata\n+                {\n+                    RepositoryOwner = body.RepositoryOwner,\n+                    RepositoryName = body.RepositoryName,\n+                    PullRequestNumber = body.PullRequestNumber\n+                };\n+\n+                var changedFiles = await _gitHubService.GetFilesDiffAsync(metadata.Token, repositoryRequestMetaData);\n+\n+                // Replace the foreach loop with parallel async processing\n+                var contentTasks = changedFiles.Select(async file =>\n+                {\n+                    var content = await _gitHubService.GetFileContentAsync(metadata.Token, file.ContentUrl!);\n+                    var decodedContent = Utils.DecodeContentBase64(content);\n+                    file.Content = decodedContent;\n+                }).ToList();\n+\n+                await Task.WhenAll(contentTasks);\n+\n+                foreach (var file in changedFiles)\n+                {\n+                    // Checking if file extension of file to review is supported\n+                    var fileExtension = Path.GetExtension(file.FileName);\n+                    if (!extensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))\n+                    {\n+                        _logger.LogWarning($\"Skipping, unsupported extension: {fileExtension} file: {file.FileName}\");\n+                        continue;\n+                    }\n+\n+                    // Checking if File content is empty\n+                    if (file.Content!.Length == 0)\n+                    {\n+                        _logger.LogWarning($\"Skipping, empty content for file: {file.FileName}\");\n+                        continue;\n+                    }\n+\n+                    // Check if File Diff is Empty\n+                    if (file.Patch!.Length == 0)\n+                        _logger.LogWarning($\"diff for file: {file.FileName} is Empty\");\n+\n+                    string separator = \"\\n\\n----------------------------------------------------------------------\\n\\n\";\n+                    _logger.LogInformation($\"Asking Review for THESSA AI... Content Length: {file.Content.Length} Diff Length: {file.Patch.Length}.\");\n+\n+                    var thessaReview = await _thessaService.ReviewAsync(file.Content, file.Patch);\n+\n+                    _logger.LogInformation($\"{separator}{file.Content}{separator}{file.Patch}{separator}{thessaReview}{separator}\");\n+\n+                    if (_thessaService.IsNoIssueText(thessaReview))\n+                        _logger.LogInformation($\"File: {file.FileName} looks good. Continue.\");\n+                    else\n+                    {\n+                        var lineComments = _thessaService.SplitReview(thessaReview);\n+\n+                        if (lineComments.Count == 0)\n+                        {\n+                            _logger.LogWarning($\"No comments found for file: {file.FileName}\");\n+                        }\n+\n+                        bool reviewResult = false;\n+                        foreach (var lineComment in lineComments)\n+                        {\n+                            try\n+                            {\n+                                if (lineComment.Line != null)\n+                                    reviewResult = await _gitHubService.PostCommentToLineAsync(\n+                                        metadata.Token, \n+                                        lineComment.Comment!, \n+                                        file.CommitId!, \n+                                        file.FileName!, \n+                                        lineComment.Line, \n+                                        repositoryRequestMetaData);\n+                                if (!reviewResult)\n+                                    reviewResult = await _gitHubService.PostGeneralCommentAsync(lineComment.Comment!, metadata.Token, repositoryRequestMetaData);\n+                            }\n+                            catch (Exception ex)\n+                            {\n+                                _logger.LogError(ex, $\"Error posting comment for file: {file.FileName} at line: {lineComment.Line}\");\n+                                throw new RepositoryException(\"Failed to post any Review.\", ex);\n+                            }\n+                        }\n+                    }\n+                }\n+                return Ok(\"Succeed\");\n+            }\n+            catch (Exception ex)\n+            {\n+                _logger.LogError(ex, \"Error processing GitHub request\");\n+                return StatusCode(StatusCodes.Status500InternalServerError, \"An error occurred while processing the request.\");\n+            }\n         }\n     }\n }",
                    code : "\ufeffusing System.Text.Json;\nusing System.Threading.Tasks;\nusing Asp.Versioning;\nusing Microsoft.AspNetCore.Http;\nusing Microsoft.AspNetCore.Mvc;\nusing THESSA.Contract;\nusing THESSA.Helper;\nusing THESSA.Models;\nusing THESSA.Repository;\n\nnamespace THESSA.Controllers\n{\n\n    [Route(\"api/v{version:apiVersion}/[controller]\")]\n    [ApiController]\n    [ApiVersion(\"1.0\")]\n    public class GitHubController : ControllerBase\n    {\n        private readonly ILogger<GitHubController> _logger;\n        private readonly IGithubService _gitHubService;\n        private readonly IThessaService _thessaService;\n        public GitHubController(\n            ILogger<GitHubController> logger,\n            IGithubService gitHubService,\n            IThessaService thessaService\n            )\n        {\n            _logger = logger ?? throw new ArgumentNullException(nameof(logger));\n            _gitHubService = gitHubService ?? throw new ArgumentNullException(nameof(gitHubService));\n            _thessaService = thessaService ?? throw new ArgumentNullException(nameof(thessaService));\n\n        }\n        [HttpPost]\n        public async Task<IActionResult> PostReview(\n            [FromHeader] GitHubMetadata metadata,\n            [FromBody] GitHubRequestBody body)\n        {\n            if (metadata == null || body == null)\n            {\n                return BadRequest(\"Metadata and body cannot be null\");\n            }\n            try\n            {\n                string[] extensions;\n                if (!string.IsNullOrEmpty(metadata.TargetExtensions))\n                    extensions = metadata.TargetExtensions\n                        .Split(',', StringSplitOptions.RemoveEmptyEntries)\n                        .Select(ext => ext.Trim())\n                        .ToArray();\n                else extensions = [\".cs\", \".py\"]; // Default file extensions\n\n                _logger.LogInformation($\"Received GitHub request with metadata: {JsonSerializer.Serialize(metadata)} and body: {JsonSerializer.Serialize(body)}\");\n                // Process the data here\n                // For example, you can log it or perform some action\n\n                var repositoryRequestMetaData = new RepositoryMetadata\n                {\n                    RepositoryOwner = body.RepositoryOwner,\n                    RepositoryName = body.RepositoryName,\n                    PullRequestNumber = body.PullRequestNumber\n                };\n\n                var changedFiles = await _gitHubService.GetFilesDiffAsync(metadata.Token, repositoryRequestMetaData);\n\n                // Replace the foreach loop with parallel async processing\n                var contentTasks = changedFiles.Select(async file =>\n                {\n                    var content = await _gitHubService.GetFileContentAsync(metadata.Token, file.ContentUrl!);\n                    var decodedContent = Utils.DecodeContentBase64(content);\n                    file.Content = decodedContent;\n                }).ToList();\n\n                await Task.WhenAll(contentTasks);\n\n                foreach (var file in changedFiles)\n                {\n                    // Checking if file extension of file to review is supported\n                    var fileExtension = Path.GetExtension(file.FileName);\n                    if (!extensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))\n                    {\n                        _logger.LogWarning($\"Skipping, unsupported extension: {fileExtension} file: {file.FileName}\");\n                        continue;\n                    }\n\n                    // Checking if File content is empty\n                    if (file.Content!.Length == 0)\n                    {\n                        _logger.LogWarning($\"Skipping, empty content for file: {file.FileName}\");\n                        continue;\n                    }\n\n                    // Check if File Diff is Empty\n                    if (file.Patch!.Length == 0)\n                        _logger.LogWarning($\"diff for file: {file.FileName} is Empty\");\n\n                    string separator = \"\\n\\n----------------------------------------------------------------------\\n\\n\";\n                    _logger.LogInformation($\"Asking Review for THESSA AI... Content Length: {file.Content.Length} Diff Length: {file.Patch.Length}.\");\n\n                    var thessaReview = await _thessaService.ReviewAsync(file.Content, file.Patch);\n\n                    _logger.LogInformation($\"{separator}{file.Content}{separator}{file.Patch}{separator}{thessaReview}{separator}\");\n\n                    if (_thessaService.IsNoIssueText(thessaReview))\n                        _logger.LogInformation($\"File: {file.FileName} looks good. Continue.\");\n                    else\n                    {\n                        var lineComments = _thessaService.SplitReview(thessaReview);\n\n                        if (lineComments.Count == 0)\n                        {\n                            _logger.LogWarning($\"No comments found for file: {file.FileName}\");\n                        }\n\n                        bool reviewResult = false;\n                        foreach (var lineComment in lineComments)\n                        {\n                            try\n                            {\n                                if (lineComment.Line != null)\n                                    reviewResult = await _gitHubService.PostCommentToLineAsync(\n                                        metadata.Token, \n                                        lineComment.Comment!, \n                                        file.CommitId!, \n                                        file.FileName!, \n                                        lineComment.Line, \n                                        repositoryRequestMetaData);\n                                if (!reviewResult)\n                                    reviewResult = await _gitHubService.PostGeneralCommentAsync(lineComment.Comment!, metadata.Token, repositoryRequestMetaData);\n                            }\n                            catch (Exception ex)\n                            {\n                                _logger.LogError(ex, $\"Error posting comment for file: {file.FileName} at line: {lineComment.Line}\");\n                                throw new RepositoryException(\"Failed to post any Review.\", ex);\n                            }\n                        }\n                    }\n                }\n                return Ok(\"Succeed\");\n            }\n            catch (Exception ex)\n            {\n                _logger.LogError(ex, \"Error processing GitHub request\");\n                return StatusCode(StatusCodes.Status500InternalServerError, \"An error occurred while processing the request.\");\n            }\n        }\n    }\n}\n"
                );

                var lineComments = _thessaService.SplitReview(response);

                bool reviewResult = false;
                //foreach (var lineComment in lineComments)
                //{
                //    try
                //    {
                //        if (lineComment.Line != null)
                //            reviewResult = await _gitHubService.PostCommentToLineAsync(
                //                metadata.Token,
                //                lineComment.Comment!,
                //                "43f8a8f93b90096e1a161ac2cb2344eb70695b9b",
                //                "Controllers/GitHubController.cs",
                //                lineComment.Line,
                //                repositoryRequestMetaData);
                //        if (!reviewResult)
                //            reviewResult = await _gitHubService.PostGeneralCommentAsync(lineComment.Comment!, metadata.Token, repositoryRequestMetaData);
                //    }
                //    catch (Exception ex)
                //    {
                //        //_logger.LogError(ex, $"Error posting comment for file: {file.FileName} at line: {lineComment.Line}");
                //        throw new RepositoryException("Failed to post any Review.", ex);
                //    }
                //}
                return Ok(lineComments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GitHub request");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the request.");
            }
        }
    }
}
