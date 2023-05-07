namespace StarBlog.Share.Utils;

public static class GuidUtils {
    /// <summary>
    /// 由连字符分隔的32位数字
    /// </summary>
    /// <returns></returns>
    private static string GetGuid() {
        var guid = new Guid();
        guid = Guid.NewGuid();
        return guid.ToString();
    }

    /// <summary>  
    /// 根据GUID获取16位的唯一字符串  
    /// </summary>  
    /// <returns></returns>  
    public static string GuidTo16String() {
        long i = 1;
        foreach (var b in Guid.NewGuid().ToByteArray()) {
            i *= b + 1;
        }

        return $"{i - DateTime.Now.Ticks:x}";
    }

    /// <summary>  
    /// 根据GUID获取19位的唯一数字序列  
    /// </summary>  
    /// <returns></returns>  
    public static long GuidToLongID() {
        var buffer = Guid.NewGuid().ToByteArray();
        return BitConverter.ToInt64(buffer, 0);
    }
}