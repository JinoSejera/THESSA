using Microsoft.SemanticKernel;
using THESSA.Contract;
using THESSA.Models;

namespace THESSA.Repository;

public class ThessaRepository : IThessaBotRepository
{
    private readonly Kernel _kernel;
    public ThessaRepository(Kernel kernel)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
    }

    public bool IsNoIssueText(string source)
    {
        var target = "No critical issues found".Replace(" ", "");
        var sourceNoSpaces = source.Replace(" ", "");
        return sourceNoSpaces.StartsWith(target);
    }

    public async Task<string> RequestDiffsAsync(string code, string diff)
    {
        var sb = new System.Text.StringBuilder();

        var arguments = new KernelArguments() { ["code"] = code, ["diffs"] = diff };

        await foreach (StreamingKernelContent response in _kernel.InvokeStreamingAsync("CommentGenerator", "PullRequestCOmmentGen", arguments))
        {
            sb.Append(response.ToString());
        }

        return sb.ToString().Trim();
    }

    public List<LineComment> SplitThessaReview(string response)
    {
        var lineComments = new List<LineComment>();
        if (string.IsNullOrEmpty(response)) return lineComments;

        var lines = response.Trim().Split('\n');
        foreach (var line in lines)
        {
            var fullLine = line.Trim();
            if (fullLine.Length == 0) continue;

            string lineNumberStr = "";
            foreach (var ch in fullLine)
            {
                if (char.IsDigit(ch)) lineNumberStr += ch;
                else break;
            }

            int lineNumber = 0;
            if(!string.IsNullOrEmpty(lineNumberStr)) int.TryParse(lineNumberStr, out lineNumber);

            lineComments.Add(new LineComment
            {
                Line = lineNumber,
                Comment = fullLine
            });
        }
        return lineComments;

    }
}
