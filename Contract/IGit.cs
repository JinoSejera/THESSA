namespace THESSA.Contract;

public interface IGit
{
    string GetRemoteName();
    string GetLastCommitSha(string file);
    List<string> GetDiffFiles(string remoteName, string headRef, string baseRef);
    string GetDiffInFile(string remoteName, string headRef, string baseRef, string filePath);

}
