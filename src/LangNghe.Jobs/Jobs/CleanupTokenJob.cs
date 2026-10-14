namespace LangNghe.Jobs.Jobs;

using LangNghe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Job xóa refresh token đã hết hạn hoặc đã revoke. Chạy 3h sáng hàng ngày.
/// </summary>
public class CleanupTokenJob
{
    private readonly ILangNgheDbContext _context;
    private readonly ILogger<CleanupTokenJob> _logger;

    public CleanupTokenJob(ILangNgheDbContext context, ILogger<CleanupTokenJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(-7);
        _logger.LogInformation("Bắt đầu cleanup refresh token cũ hơn {Cutoff}", cutoff);

        var deleted = await _context.RefreshTokens
            .Where(x => x.ExpiresAt < cutoff || x.RevokedAt != null)
            .ExecuteDeleteAsync();

        _logger.LogInformation("Đã xóa {Count} refresh token", deleted);
    }
}
