namespace DataProc.Utilities;

using System.Text.Encodings.Web;
using System.Text.Json;

public static class Json {
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    
    public static string Dumps(object data) => JsonSerializer.Serialize(data, Options);
}