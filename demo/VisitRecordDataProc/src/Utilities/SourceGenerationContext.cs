using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using VisitRecordDataProc.Entities;

namespace VisitRecordDataProc.Utilities;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(OutputResult))]
internal partial class SourceGenerationContext : JsonSerializerContext { }