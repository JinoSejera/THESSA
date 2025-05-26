namespace THESSA.Models
{
    public class PostCommentToLineRequestBody
    {
        public string? body { get; set; }
        public string? commit_id { get; set; }
        public string? path { get; set; }
        public int position { get; set; }
    }
}
