using Microsoft.AspNetCore.Mvc;
using StarBlog.Web.Extensions;

namespace StarBlog.Web.Apis.Test;

/// <summary>
/// 用于测试错误处理能力
/// </summary>
[ApiController]
[Route("Api/[controller]/[action]")]
[ApiExplorerSettings(GroupName = ApiGroups.Test)]
public class ErrorTestController : ControllerBase {
    [HttpGet]
    public IActionResult Error() {
        throw new Exception();
    }
}