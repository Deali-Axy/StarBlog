namespace StarBlog.Application.ViewModels.Config;

public class ConfigItemCreationDto {
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}
