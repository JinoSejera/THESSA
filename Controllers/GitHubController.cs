using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using THESSA.Models;

namespace THESSA.Controllers
{

    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class GitHubController : ControllerBase
    {
        [HttpPost]
        public IActionResult PostReview(
            [FromHeader] GitHubMetadata metadata,
            [FromBody] GitHubRequestBody body)
        {
            if (metadata == null || body == null)
            {
                return BadRequest("Metadata and body cannot be null");
            }
            // Process the data here
            // For example, you can log it or perform some action
            return Ok("Data received successfully");
        }
    }
}
