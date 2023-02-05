namespace StarBlog.Share.Extensions; 

public static class StringExt {
    public static string Limit(this string str,int length) {
        if (str.Length<=length) {
            return str;
        }

        return str[..length];
    }
}