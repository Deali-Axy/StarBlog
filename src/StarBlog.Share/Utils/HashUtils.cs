using System.Security.Cryptography;
using System.Text;

namespace StarBlog.Share.Utils;

/// <summary>
/// 哈希工具
/// </summary>
public static class HashUtils {
    private static string ComputeHash(HashAlgorithm hashAlgorithm, string source) {
        var data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(source));
        var sb = new StringBuilder();
        foreach (var item in data) {
            sb.Append(item.ToString("x2"));
        }

        // Return the hexadecimal string.
        return sb.ToString();
    }

    public static string ComputeSHA256Hash(string source) {
        using var sha256 = SHA256.Create();
        return ComputeHash(sha256, source);
    }

    public static string ComputeSHA384Hash(string source) {
        using var algo = SHA384.Create();
        return ComputeHash(algo, source);
    }
    
    public static string ComputeSHA512Hash(string source) {
        using var algo = SHA512.Create();
        return ComputeHash(algo, source);
    }
}