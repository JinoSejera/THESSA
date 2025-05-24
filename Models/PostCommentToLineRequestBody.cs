namespace THESSA.Models
{
    public class PostCommentToLineRequestBody
    {
        public string? body { get; set; }
        public int commit_id { get; set; }
        public string? path { get; set; }
        public int position { get; set; }
    }
}
