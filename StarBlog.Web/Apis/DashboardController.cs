using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Contrib.CLRStats;
using StarBlog.Web.ViewModels.Response;

namespace StarBlog.Web.Apis;

[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = "common")]
public class DashboardController : ControllerBase {
    [HttpGet("[action]")]
    public ApiResponse ClrStats() {
        return ApiResponse.Ok(CLRStatsUtils.GetCurrentClrStats());
    }
}