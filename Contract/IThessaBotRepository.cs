using THESSA.Models;

namespace THESSA.Contract
{
    public interface IThessaBotRepository
    {
        Task<string> RequestDiffs(string code, string diff);
        List<LineComment> SplitThessaReview(string response);
        bool IsNoIssueText(string source);

    }
}
