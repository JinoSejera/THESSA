using THESSA.Contract;
using THESSA.Models;

namespace THESSA.Services
{
    public class ThessaService : IThessaService
    {
        private readonly ILogger<ThessaService> _logger;
        private readonly IThessaBotRepository _thessaRepos;
        public ThessaService(
            ILogger<ThessaService> logger,
            IThessaBotRepository thessaRepos)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _thessaRepos = thessaRepos ?? throw new ArgumentNullException(nameof(thessaRepos));
        }
        public bool IsNoIssueText(string source)
        {
            try
            {
                return _thessaRepos.IsNoIssueText(source);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when checking if source is no issue.");
                throw;
            }
        }

        public async Task<string> ReviewAsync(string code, string diff)
        {
            try
            {
                return await _thessaRepos.RequestDiffsAsync(code, diff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when requesting Thessa review.");
                throw;
            }
        }

        public List<LineComment> SplitReview(string response)
        {
            try
            {
                return _thessaRepos.SplitThessaReview(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when splitting Thessa review response.");
                throw;
            }
        }
    }
}
