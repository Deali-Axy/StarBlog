using System.Text.Json.Serialization;

namespace StarBlog.Web.Models; 

public class Hitokoto {
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("hitokoto")]
    public string Content { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("from_where")]
    public string FromWhere { get; set; }

    [JsonPropertyName("creator")]
    public string Creator { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}