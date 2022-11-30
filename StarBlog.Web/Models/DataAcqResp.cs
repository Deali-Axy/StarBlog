using System.Text.Json.Serialization;

namespace StarBlog.Web.Models; 

public class DataAcqResp<T> {
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("msg")]
    public string Msg { get; set; }

    [JsonPropertyName("data")]
    public T Data { get; set; }
}