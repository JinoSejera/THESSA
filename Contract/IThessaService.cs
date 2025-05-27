using THESSA.Models;

namespace THESSA.Contract
{
    public interface IThessaService
    {
        Task<string> ReviewAsync(string code, string diff);
        List<LineComment> SplitReview(string response);
        bool IsNoIssueText(string source);
    }
}
