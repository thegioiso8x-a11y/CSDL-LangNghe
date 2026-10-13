using System;
using LangNghe.Domain.Interfaces;

namespace LangNghe.Infrastructure.Services;

/// <summary>
/// Implementation của IDateTimeProvider — trả thời gian UTC chuẩn.
/// </summary>
public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
}
