namespace LangNghe.Jobs.Jobs;

using LangNghe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Job xóa audit log cũ hơn 2 năm (retention policy). Chạy 4h sáng hàng ngày.
/// </summary>
public class CleanupAuditJob
{
    private readonly ILangNgheDbContext _context;
    private readonly ILogger<CleanupAuditJob> _logger;

    public CleanupAuditJob(ILangNgheDbContext context, ILogger<CleanupAuditJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(-730);
        _logger.LogInformation("Bắt đầu cleanup audit log cũ hơn {Cutoff}", cutoff);

        var deleted = await _context.AuditLogs
            .Where(x => x.CreatedAt < cutoff)
            .ExecuteDeleteAsync();

        _logger.LogInformation("Đã xóa {Count} audit log", deleted);
    }
}
