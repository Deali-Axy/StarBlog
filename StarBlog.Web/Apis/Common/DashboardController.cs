using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Contrib.CLRStats;

namespace StarBlog.Web.Apis.Common;

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