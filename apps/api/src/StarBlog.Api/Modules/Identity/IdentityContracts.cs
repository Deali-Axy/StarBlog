using System.ComponentModel.DataAnnotations;

namespace StarBlog.Api.Modules.Identity;

/// <summary>登录请求。</summary>
public sealed class LoginRequest {
    [Required, StringLength(64, MinimumLength = 3)]
    public string Username { get; init; } = string.Empty;

    [Required, StringLength(256, MinimumLength = 1)]
    public string Password { get; init; } = string.Empty;
}

/// <summary>登录成功后返回的 JWT。</summary>
public sealed class LoginTokenResponse {
    public required string Token { get; init; }
    public required DateTime Expiration { get; init; }
}

/// <summary>当前用户资料，不包含密码。</summary>
public sealed class CurrentUserResponse {
    public required string Id { get; init; }
    public required string Name { get; init; }
}

/// <summary>首次初始化请求。仅在用户表为空时允许调用。</summary>
public sealed class InitializeSiteRequest {
    [Required, StringLength(64, MinimumLength = 3)]
    public string Username { get; init; } = string.Empty;

    [Required, StringLength(256, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;

    [Required, Url]
    public string Host { get; init; } = string.Empty;

    [Required]
    public string DefaultRender { get; init; } = "frontend";
}

/// <summary>初始化状态，供管理端决定展示登录还是首次创建表单。</summary>
public sealed class InitializationStateResponse {
    public required bool IsInitialized { get; init; }
    public string? Host { get; init; }
    public string? DefaultRender { get; init; }
}
