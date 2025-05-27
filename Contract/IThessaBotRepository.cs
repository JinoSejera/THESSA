using THESSA.Models;

namespace THESSA.Contract
{
    public interface IThessaBotRepository
    {
        Task<string> RequestDiffsAsync(string code, string diff);
        List<LineComment> SplitThessaReview(string response);
        bool IsNoIssueText(string source);

    }
}
