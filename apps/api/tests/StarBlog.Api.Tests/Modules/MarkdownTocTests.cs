using StarBlog.Api.Modules.Content.Markdown;
using StarBlog.Api.Modules.Analytics;

namespace StarBlog.Api.Tests.Modules;

public sealed class MarkdownTocTests {
    [Fact]
    public void Null_content_returns_null() {
        Assert.Null(TableOfContents.Extract(null));
    }

    [Fact]
    public void Single_heading_produces_one_node() {
        var toc = TableOfContents.Extract("# Hello");
        Assert.NotNull(toc);
        Assert.Single(toc);
        Assert.Equal("Hello", toc![0].Text);
        Assert.StartsWith("#", toc[0].Href);
    }

    [Fact]
    public void Nested_headings_build_a_tree() {
        var toc = TableOfContents.Extract("# A\n## B\n### C\n## D\n# E");
        Assert.NotNull(toc);
        Assert.Equal(2, toc!.Count);
        Assert.Equal("A", toc[0].Text);
        Assert.Equal(2, toc[0].Nodes!.Count);
        Assert.Equal("B", toc[0].Nodes![0].Text);
        Assert.Equal("C", toc[0].Nodes![0].Nodes![0].Text);
        Assert.Equal("D", toc[0].Nodes![1].Text);
        Assert.Equal("E", toc[1].Text);
    }

    [Fact]
    public void Fake_ip_searcher_returns_placeholder() {
        using var searcher = new FakeIpSearcher();
        Assert.Equal(FakeIpSearcher.FakeResult, searcher.Search("1.1.1.1"));
        Assert.Equal(0, searcher.IoCount);
    }
}
