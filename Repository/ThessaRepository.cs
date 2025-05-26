using Microsoft.SemanticKernel;
using THESSA.Contract;

namespace THESSA.Repository;

public class ThessaRepository : IThessaBotRepository
{
    private readonly Kernel _kernel;

    public ThessaRepository(Kernel kernel)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
    }

    public async Task<string> RequestDiffs(string code, string diff)
    {
        string[] content = [];

        var arguments = new KernelArguments() { ["code"] = code, ["diffs"] = diff
        };

        var result = _kernel.InvokeStreamingAsync("CommentGenerator", "PullRequestCOmmentGen", arguments);

        await foreach (StreamingKernelContent response in _kernel.InvokeStreamingAsync("CommentGenerator", "PullRequestCOmmentGen", arguments))
        {
            content.Append(response.ToString());
        }

        return string.Join("", content).Trim();
    }
}
