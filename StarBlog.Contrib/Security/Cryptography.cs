using System.Security.Cryptography;
using System.Text;

namespace StarBlog.Contrib.Security; 

/// <summary>
/// 密码学相关
/// </summary>
public static class Cryptography {
    public static string ToMd5String(this string source) {
        using (var sha256 = SHA256.Create()) {
            var data = sha256.ComputeHash(Encoding.UTF8.GetBytes(source));
            var sb = new StringBuilder();
            foreach (var item in data) {
                sb.Append(item.ToString("x2"));
            }
            
            // Return the hexadecimal string.
            return sb.ToString();
        }
    }
}