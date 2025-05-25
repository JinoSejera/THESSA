using Microsoft.SemanticKernel;
using THESSA.Controllers;

namespace THESSA.Repository;

public class ThessRepository : IThessBotRepository
{
    private readonly Kernel _kernel;

    public ThessRepository(Kernel kernel)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
    }

    public Task<string> RequestDiffs(string code, string diff)
    {
        string[] content = [];

        var arguments = new KernelArguments() { ["code"] = code, ["diffs"] = diff
        };

        var result = _kernel.InvokeStreamingAsync("CommentGenerator", "PullRequestCOmmentGen", arguments);

        return Task.FromResult("");
    }
}
