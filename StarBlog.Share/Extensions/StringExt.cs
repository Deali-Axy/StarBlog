using StarBlog.Share.Utils;

namespace StarBlog.Share.Extensions;

public static class StringExt {
    public static string Limit(this string str, int length) {
        if (str.Length <= length) {
            return str;
        }

        return str[..length];
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