using THESSA.Models;

namespace THESSA.Controllers
{
    public interface IThessBotRepository
    {
        Task<string> RequestDiffs(string code, string diff);

        private static readonly string NoResponse = "No critical issues found";
        private static readonly string Problems = "errors, issues, potential crashes or unhandled exceptions";
        private static readonly string ThessQuery = @"
# TASK
Could you describe briefly {0}, for the next code with the given git diffs?
Please, also do not add introduction words, just print errors in the format: ""line_number: cause effect""
If there are no {0} just say ""{1}"".

## DIFFS:
{2}

## Full code from the file:
{3}
";
        static string BuilThessQuery(string code, string diffs) =>
            string.Format(ThessQuery, Problems, NoResponse, diffs, code);

        static bool IsNoIssueText(string source)
        {
            var target = NoResponse.Replace(" ", "");
            var sourceNoSpaces = source.Replace(" ", "");
            return sourceNoSpaces.StartsWith(target);
        }

        static List<LineComment> SplitThessResponse(string response)
        {
            var result = new List<LineComment>();
            if(string.IsNullOrEmpty(response)) return result;

            var lines = response.Trim().Split('\n');
            foreach (var fullRawText in lines)
            {
                var fullText = fullRawText.Trim();
                if (fullText.Length == 0) continue;

                string numberStr = "";
                foreach (var ch in fullText)
                {
                    if (char.IsDigit(ch)) numberStr += ch;
                    else break;
                }

                int number = 0;
                if (!string.IsNullOrEmpty(numberStr)) int.TryParse(numberStr, out number);

                result.Add(new LineComment { Line = number, Comment = fullText});
            }
            return result;
        }
    }
}
