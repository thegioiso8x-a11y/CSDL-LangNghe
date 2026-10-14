using System.Reflection;
using FluentValidation;
using LangNghe.Application.Behaviors;
using LangNghe.Application.Features.Import.Interfaces;
using LangNghe.Application.Features.Import.Services;
using LangNghe.Application.Features.Aggregation.Interfaces;
using LangNghe.Application.Features.Aggregation.Services;
using LangNghe.Application.Features.User.Interfaces;
using LangNghe.Application.Features.User.Services;
using LangNghe.Application.Features.Notifications.Interfaces;
using LangNghe.Application.Features.Notifications.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LangNghe.Application;

/// <summary>
/// Cấu hình Dependency Injection cho tầng Application.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            
            // Đăng ký behaviors theo thứ tự: Logging -> Validation
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddMemoryCache();

        services.AddScoped<IExcelTemplateService, ExcelTemplateService>();
        services.AddScoped<IExcelImportService, ExcelImportService>();
        services.AddScoped<IAggRebuildService, AggRebuildService>();
        services.AddScoped<IDashboardCacheService, DashboardCacheService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IUserImportService, UserImportService>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
