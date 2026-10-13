using LangNghe.Application.Features.Auth.Interfaces;
using LangNghe.Application.Interfaces;
using LangNghe.Domain.Interfaces;
using LangNghe.Infrastructure.Persistence;
using LangNghe.Infrastructure.Persistence.Interceptors;
using LangNghe.Infrastructure.Repositories;
using LangNghe.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LangNghe.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IHttpContextInfo, HttpContextInfo>();

        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<AuditLogInterceptor>();

        services.AddDbContext<LangNgheDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>());
            options.AddInterceptors(sp.GetRequiredService<AuditLogInterceptor>());
        });

        services.AddScoped<ILangNgheDbContext>(sp => sp.GetRequiredService<LangNgheDbContext>());

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICaptchaService, CaptchaService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IVillageOwnershipService, VillageOwnershipService>();

        return services;
    }
}