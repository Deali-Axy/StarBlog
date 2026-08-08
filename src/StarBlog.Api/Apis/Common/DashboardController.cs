using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Infrastructure.CLRStats;
using StarBlog.Api.Extensions;

namespace StarBlog.Api.Apis.Common;

[Authorize]
[ApiController]
[Route("Api/[controller]")]
[Route("Api/Admin/Dashboard")]
[ApiExplorerSettings(GroupName = ApiGroups.Admin)]
public class DashboardController : ControllerBase {
    [HttpGet("[action]")]
    public ApiResponse ClrStats() {
        return ApiResponse.Ok(CLRStatsUtils.GetCurrentClrStats());
    }
}
