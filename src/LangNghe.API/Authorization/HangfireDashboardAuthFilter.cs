namespace LangNghe.API.Authorization;

using Hangfire.Dashboard;

/// <summary>
/// Filter cho phép truy cập Hangfire dashboard.
/// Dev: cho phép tất cả (tiện test).
/// Production: yêu cầu đăng nhập + DataScope=1 (TW).
/// </summary>
public class HangfireDashboardAuthFilter : IDashboardAuthorizationFilter
{
    private readonly IWebHostEnvironment _env;

    public HangfireDashboardAuthFilter(IWebHostEnvironment env)
    {
        _env = env;
    }

    public bool Authorize(DashboardContext context)
    {
        if (_env.IsDevelopment()) return true;

        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.FindFirst("dataScope")?.Value == "1";
    }
}
