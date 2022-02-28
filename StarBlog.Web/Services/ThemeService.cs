namespace StarBlog.Web.Services;

public class ThemeService {
    private const string CssUrlPrefix = "/lib/bootswatch/dist";
    public List<Theme> Themes { get; set; } = new List<Theme>();

    public ThemeService(IWebHostEnvironment env) {
        var themePath = Path.Combine(env.WebRootPath, "lib", "bootswatch", "dist");
        foreach (var item in Directory.GetDirectories(themePath)) {
            var name = Path.GetFileName(item);
            Themes.Add(new Theme {
                Name = name,
                Path = item,
                CssUrl = $"{CssUrlPrefix}/{name}/bootstrap.min.css"
            });
        }
    }
}

public class Theme {
    public string Name { get; set; }
    public string Path { get; set; }
    public string CssUrl { get; set; }
}