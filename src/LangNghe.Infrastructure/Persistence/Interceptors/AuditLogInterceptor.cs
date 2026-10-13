namespace LangNghe.Infrastructure.Persistence.Interceptors;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Interfaces;
using LangNghe.Domain.Entities;
using LangNghe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
/// Interceptor tự động ghi lại nhật ký thay đổi (Audit Log) cho các thao tác CUD.
/// </summary>
public class AuditLogInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextInfo _httpContextInfo;
    private bool _isCapturing;

    public AuditLogInterceptor(ICurrentUserService currentUser, IHttpContextInfo httpContextInfo)
    {
        _currentUser = currentUser;
        _httpContextInfo = httpContextInfo;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        CaptureAuditLogs(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CaptureAuditLogs(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void CaptureAuditLogs(DbContext? context)
    {
        if (context == null || _isCapturing)
        {
            return;
        }

        try
        {
            _isCapturing = true;

            var auditEntries = new List<AuditLog>();
            var entries = context.ChangeTracker.Entries().ToList();

            foreach (var entry in entries)
            {
                if (entry.Entity is AuditLog)
                {
                    continue;
                }

                var entityName = entry.Metadata.ClrType.Name;
                if (ShouldSkip(entityName))
                {
                    continue;
                }

                if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                {
                    continue;
                }

                string action = entry.State switch
                {
                    EntityState.Added => "Create",
                    EntityState.Modified => "Update",
                    EntityState.Deleted => "Delete",
                    _ => string.Empty
                };

                var entityId = GetEntityId(entry);

                var oldValues = new Dictionary<string, object?>();
                var newValues = new Dictionary<string, object?>();

                if (entry.State == EntityState.Added)
                {
                    foreach (var prop in entry.Properties)
                    {
                        newValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    foreach (var prop in entry.Properties)
                    {
                        if (prop.IsModified)
                        {
                            oldValues[prop.Metadata.Name] = prop.OriginalValue;
                            newValues[prop.Metadata.Name] = prop.CurrentValue;
                        }
                    }
                }
                else if (entry.State == EntityState.Deleted)
                {
                    foreach (var prop in entry.Properties)
                    {
                        oldValues[prop.Metadata.Name] = prop.OriginalValue;
                    }
                }

                var ipAddress = _httpContextInfo.IpAddress;
                if (ipAddress?.Length > 50)
                {
                    ipAddress = ipAddress.Substring(0, 50);
                }

                var userAgent = _httpContextInfo.UserAgent;
                if (userAgent?.Length > 500)
                {
                    userAgent = userAgent.Substring(0, 500);
                }

                if (entityId?.Length > 50)
                {
                    entityId = entityId.Substring(0, 50);
                }

                var auditLog = new AuditLog
                {
                    UserId = _currentUser.UserId,
                    Username = _currentUser.Username,
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    OldValues = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null,
                    NewValues = newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    CreatedAt = DateTime.UtcNow
                };

                auditEntries.Add(auditLog);
            }

            if (auditEntries.Count > 0)
            {
                if (context is ILangNgheDbContext dbContext)
                {
                    dbContext.AuditLogs.AddRange(auditEntries);
                }
                else
                {
                    context.Set<AuditLog>().AddRange(auditEntries);
                }
            }
        }
        finally
        {
            _isCapturing = false;
        }
    }

    private static bool ShouldSkip(string entityName)
    {
        if (entityName == nameof(AuditLog))
        {
            return true;
        }

        if (entityName.StartsWith("IndustryGroupAggregate", StringComparison.OrdinalIgnoreCase) ||
            entityName.StartsWith("VillageTypeAggregate", StringComparison.OrdinalIgnoreCase) ||
            entityName.StartsWith("VillageTimeSeries", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (entityName is "ImportHistory" or "RawImportStaging" or "ImportErrorLog" or "RefreshToken")
        {
            return true;
        }

        return false;
    }

    private static string? GetEntityId(EntityEntry entry)
    {
        var keyProps = entry.Properties.Where(p => p.Metadata.IsPrimaryKey()).ToList();
        if (keyProps.Count == 1)
        {
            var val = entry.State == EntityState.Deleted
                ? keyProps[0].OriginalValue
                : keyProps[0].CurrentValue;
            return val?.ToString();
        }

        if (keyProps.Count > 1)
        {
            return string.Join(",", keyProps.Select(p =>
                (entry.State == EntityState.Deleted ? p.OriginalValue : p.CurrentValue)?.ToString() ?? ""));
        }

        var idProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
        if (idProp != null)
        {
            var val = entry.State == EntityState.Deleted ? idProp.OriginalValue : idProp.CurrentValue;
            return val?.ToString();
        }

        return null;
    }
}
