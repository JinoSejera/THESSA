using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace THESSA.Models
{
    public class GitHubMetadata
    {
        [Required]
        [FromHeader(Name = "token")]
        public string Token { get; set; }

        [Required]
        [FromHeader(Name = "targetExtensions")]
        public string TargetExtensions { get; set; }
    }
}
