using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.Response;

namespace StarBlog.Web.Apis;

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

    [HttpGet("{id:int}")]
    public ApiResponse<ConfigItem> GetById(int id) {
        var item = _service.GetById(id);
        return item == null ? ApiResponse.NotFound() : new ApiResponse<ConfigItem>(item);
    }

    [HttpGet("{key}")]
    public ApiResponse<ConfigItem> GetByKey(string key) {
        var item = _service.GetByKey(key);
        return item == null ? ApiResponse.NotFound() : new ApiResponse<ConfigItem>(item);
    }
}