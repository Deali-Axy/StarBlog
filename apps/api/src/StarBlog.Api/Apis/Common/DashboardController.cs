using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Infrastructure.CLRStats;
using StarBlog.Api.Extensions;

namespace StarBlog.Api.Apis.Common;

[Authorize]
[ApiController]
[Route("api/v1/admin/runtime")]
[ApiExplorerSettings(GroupName = ApiGroups.Admin)]
public class DashboardController : ControllerBase {
    [HttpGet("statistics")]
    public ApiResponse ClrStats() {
        return ApiResponse.Ok(CLRStatsUtils.GetCurrentClrStats());
    }
}
