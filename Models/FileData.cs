namespace THESSA.Models
{
    public class FileData
    {
        public string? FileName { get; set; }
        public string? ContentUrl { get; set; }
        public string? Patch { get; set; }
        public string? CommitId { get; set; }
        public string? Content { get; set; } = string.Empty;

    }
}
