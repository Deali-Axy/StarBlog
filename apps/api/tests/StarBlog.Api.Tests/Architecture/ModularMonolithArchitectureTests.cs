namespace StarBlog.Api.Tests.Architecture;

/// <summary>
/// 禁止在模块化单体中重新引入全局分层堆放目录。
/// </summary>
public sealed class ModularMonolithArchitectureTests {
    private static readonly string ApiRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "StarBlog.Api"));

    [Theory]
    [InlineData("Services")]
    [InlineData("DTOs")]
    [InlineData("ViewModels")]
    [InlineData("Helpers")]
    [InlineData("Repositories")]
    public void Forbidden_global_directories_do_not_exist(string name) {
        var path = Path.Combine(ApiRoot, name);
        Assert.False(Directory.Exists(path), $"不应存在全局目录 {path}");
    }

    [Fact]
    public void Source_does_not_reference_FreeSql() {
        var files = Directory.GetFiles(ApiRoot, "*.cs", SearchOption.AllDirectories);
        var hits = files
            .SelectMany(file => File.ReadAllLines(file).Select((line, index) => (file, line, index)))
            .Where(item => item.line.Contains("FreeSql", StringComparison.OrdinalIgnoreCase)
                           || item.line.Contains("IBaseRepository", StringComparison.Ordinal)
                           || item.line.Contains("SyncStructure", StringComparison.Ordinal))
            .ToList();
        Assert.True(hits.Count == 0, string.Join(Environment.NewLine, hits.Select(item => $"{item.file}:{item.index + 1}: {item.line.Trim()}")));
    }
}
