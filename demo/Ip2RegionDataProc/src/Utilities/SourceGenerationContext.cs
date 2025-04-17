using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Ip2RegionDataProc.Entities;

namespace Ip2RegionDataProc.Utilities;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(OutputResult))]
internal partial class SourceGenerationContext : JsonSerializerContext { }