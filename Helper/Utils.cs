using System.Text;

namespace THESSA.Helper;

public static class Utils
{
    public static string DecodeContentBase64(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) 
            throw new ArgumentNullException("Content cannot be null or Empty", nameof(content));

        try
        {
            byte[] data = Convert.FromBase64String(content);
            return Encoding.UTF8.GetString(data);
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Content is not a valid Base64 string", ex);
        }
    }
}
