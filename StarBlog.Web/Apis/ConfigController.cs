using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.Response;

namespace StarBlog.Web.Apis;

/// <summary>
/// 配置中心
/// </summary>
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = "admin")]
public class ConfigController : ControllerBase {
    private readonly ConfigService _service;

    public ConfigController(ConfigService service) {
        _service = service;
    }

    [HttpGet]
    public List<ConfigItem> GetAll() {
        return _service.GetAll();
    }

    [HttpGet("{key}")]
    public ApiResponse<ConfigItem> GetByKey(string key) {
        var item = _service.GetByKey(key);
        return item == null ? ApiResponse.NotFound() : new ApiResponse<ConfigItem>(item);
    }

    [HttpPost]
    public ConfigItem Add(ConfigItemCreationDto dto) {
        return _service.AddOrUpdate(new ConfigItem {
            Key = dto.Key,
            Value = dto.Value,
            Description = dto.Description
        });
    }

    [HttpPut("{key}")]
    public ApiResponse<ConfigItem> Update(string key, ConfigItemUpdateDto dto) {
        var item = _service.GetByKey(key);
        if (item == null) return ApiResponse.NotFound();
        item.Value = dto.Value;
        item.Description = dto.Description;
        return new ApiResponse<ConfigItem>(_service.AddOrUpdate(item));
    }
}