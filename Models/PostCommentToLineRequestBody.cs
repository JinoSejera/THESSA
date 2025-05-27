namespace THESSA.Models
{
    public class PostCommentToLineRequestBody
    {
        public string? body { get; set; }
        public string? commit_id { get; set; }
        public string? path { get; set; }
        public int line { get; set; }
    }
}
