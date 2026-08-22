using System.Security.Cryptography;
using System.Text;

namespace Praedora.Core;

// Used to detect when a job posting's description text changes between captures.
public static class JobDescriptionHasher
{
    public static string ComputeHash(string rawJobDescription)
    {
        string normalized = Normalize(rawJobDescription);
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string Normalize(string rawJobDescription)
    {
        string collapsedWhitespace = string.Join(' ', rawJobDescription.Split(
            (char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return collapsedWhitespace.Trim().ToLowerInvariant();
    }
}
