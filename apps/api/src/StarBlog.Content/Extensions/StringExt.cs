using StarBlog.Content.Utils;

namespace StarBlog.Content.Extensions;

public static class StringExt {
    public static string Limit(this string str, int length) {
        return str.Length <= length ? str : str[..length];
    }

    /// <summary>
    /// 限制字符串显示长度并在末尾添加省略号
    /// </summary>
    public static string LimitWithEllipsis(this string str, int length) {
        return str.Length <= length ? str : $"{str[..length]}...";
    }

    public static string ToSHA256(this string source) {
        return HashUtils.ComputeSHA256Hash(source);
    }

    public static string ToSHA384(this string source) {
        return HashUtils.ComputeSHA384Hash(source);
    }

    public static string ToSHA512(this string source) {
        return HashUtils.ComputeSHA512Hash(source);
    }
}
