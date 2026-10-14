using System.Text.Json.Serialization;
using Hangfire;
using LangNghe.API.Extensions;
using LangNghe.API.Middleware;
using LangNghe.API.Services;
using LangNghe.Application;
using LangNghe.Domain.Constants;
using LangNghe.Domain.Interfaces;
using LangNghe.Infrastructure;
using LangNghe.Jobs;
using LangNghe.API.Authorization;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseWebRoot("wwwroot");

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/langnghe-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomSwaggerGen();

// Add Application and Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var hangfireConnection = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddHangfireJobs(hangfireConnection);

// Add HttpContextAccessor and CurrentUserService
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add Jwt Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DimManage",
        p => p.RequireClaim("permission", PermissionCodes.DimManage));
    options.AddPolicy("VillageCreate",
        p => p.RequireClaim("permission", PermissionCodes.VillageCreate));
    options.AddPolicy("VillageUpdate",
        p => p.RequireClaim("permission", PermissionCodes.VillageUpdate));
    options.AddPolicy("VillageDelete",
        p => p.RequireClaim("permission", PermissionCodes.VillageDelete));
    options.AddPolicy("VillagePublish",
        p => p.RequireClaim("permission", PermissionCodes.VillagePublish));
    options.AddPolicy("VillageRead",
        p => p.RequireClaim("permission", PermissionCodes.VillageRead));
    options.AddPolicy("VillageApprove",
        p => p.RequireClaim("permission", PermissionCodes.VillageApprove));
    options.AddPolicy("ImportUpload",
        p => p.RequireClaim("permission", PermissionCodes.ImportUpload));
    options.AddPolicy("ImportRollback",
        p => p.RequireClaim("permission", PermissionCodes.ImportRollback));
    options.AddPolicy("ReportView",
        p => p.RequireClaim("permission", PermissionCodes.ReportView));
    options.AddPolicy("ReportExport",
        p => p.RequireClaim("permission", PermissionCodes.ReportExport));
    options.AddPolicy("UserManage",
        p => p.RequireClaim("permission", PermissionCodes.UserManage));
    options.AddPolicy("AuditView",
        p => p.RequireClaim("permission", PermissionCodes.AuditView));
    options.AddPolicy("ConfigManage",
        p => p.RequireClaim("permission", PermissionCodes.ConfigManage));
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllDev", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Serve static files từ wwwroot (ảnh upload)
app.UseStaticFiles();

// Configure the HTTP request pipeline.
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAllDev");

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireDashboardAuthFilter(app.Environment) },
    DashboardTitle = "LangNghe Background Jobs"
});

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.ScheduleHangfireJobs();
}

app.Run();
