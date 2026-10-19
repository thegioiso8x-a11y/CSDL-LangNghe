namespace LangNghe.API.Controllers;

using LangNghe.Application.Common;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using LangNghe.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Controller TẠM để apply migration SQL. Sẽ xóa sau giai đoạn phát triển.
/// </summary>
[ApiController]
[Route("api/v1/dev/migration")]
[Authorize]
public class DevMigrationController : ControllerBase
{
    private readonly LangNgheDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DevMigrationController(LangNgheDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Tạo bảng Sys_Notifications nếu chưa tồn tại. Chỉ TW.
    /// </summary>
    [HttpPost("notifications")]
    public async Task<ActionResult<ApiResponse<bool>>> ApplyNotificationsMigration(CancellationToken ct)
    {
        if (_currentUser.DataScope != 1)
            throw new ForbiddenException("Chỉ cấp Trung ương mới có quyền chạy migration.");

        var sql = @"
IF OBJECT_ID('dbo.Sys_Notifications','U') IS NULL
BEGIN
    CREATE TABLE Sys_Notifications (
        Id              BIGINT IDENTITY(1,1) NOT NULL,
        UserId          INT             NOT NULL,
        Title           NVARCHAR(255)   NOT NULL,
        Message         NVARCHAR(1000)  NULL,
        Type            VARCHAR(50)     NOT NULL,
        EntityType      VARCHAR(100)    NULL,
        EntityId        VARCHAR(50)     NULL,
        LinkUrl         VARCHAR(500)    NULL,
        IsRead          BIT             NOT NULL DEFAULT 0,
        ReadAt          DATETIME        NULL,
        CreatedAt       DATETIME        NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Sys_Notifications PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Notification_User FOREIGN KEY (UserId) REFERENCES Sys_Users(Id)
    );

    CREATE NONCLUSTERED INDEX IX_Notification_User_Read 
    ON Sys_Notifications(UserId, IsRead, CreatedAt DESC) 
    INCLUDE (Type, Title, EntityType, EntityId);
END
";
        await _context.Database.ExecuteSqlRawAsync(sql, ct);
        return Ok(ApiResponse<bool>.Ok(true, "Đã apply migration Notifications"));
    }

    /// <summary>
    /// Thêm cột ImageUrl cho Dim_CraftVillages và Dim_Artisans nếu chưa tồn tại.
    /// </summary>
    [HttpPost("image-urls")]
    public async Task<ActionResult<ApiResponse<bool>>> ApplyImageUrlMigration(CancellationToken ct)
    {
        if (_currentUser.DataScope != 1)
            throw new ForbiddenException("Chỉ cấp Trung ương mới có quyền chạy migration.");

        var sql = @"
IF COL_LENGTH('dbo.Dim_CraftVillages', 'ImageUrl') IS NULL
    ALTER TABLE Dim_CraftVillages ADD ImageUrl NVARCHAR(500) NULL;

IF COL_LENGTH('dbo.Dim_Artisans', 'ImageUrl') IS NULL
    ALTER TABLE Dim_Artisans ADD ImageUrl NVARCHAR(500) NULL;
";
        await _context.Database.ExecuteSqlRawAsync(sql, ct);
        return Ok(ApiResponse<bool>.Ok(true, "Đã apply migration ImageUrl"));
    }

    /// <summary>
    /// Thêm cột tọa độ cho Dim_CraftVillages.
    /// </summary>
    [HttpPost("village-coordinates")]
    public async Task<ActionResult<ApiResponse<bool>>> ApplyCoordinatesMigration(CancellationToken ct)
    {
        if (_currentUser.DataScope != 1)
            throw new ForbiddenException("Chỉ cấp Trung ương mới có quyền chạy migration.");

        var sql = @"
IF COL_LENGTH('dbo.Dim_CraftVillages', 'Latitude') IS NULL
    ALTER TABLE Dim_CraftVillages ADD Latitude DECIMAL(10,7) NULL;
IF COL_LENGTH('dbo.Dim_CraftVillages', 'Longitude') IS NULL
    ALTER TABLE Dim_CraftVillages ADD Longitude DECIMAL(10,7) NULL;
IF COL_LENGTH('dbo.Dim_CraftVillages', 'GeocodedAt') IS NULL
    ALTER TABLE Dim_CraftVillages ADD GeocodedAt DATETIME NULL;
IF COL_LENGTH('dbo.Dim_CraftVillages', 'GeocodeSource') IS NULL
    ALTER TABLE Dim_CraftVillages ADD GeocodeSource VARCHAR(50) NULL;
";
        await _context.Database.ExecuteSqlRawAsync(sql, ct);
        return Ok(ApiResponse<bool>.Ok(true, "Đã apply migration Village coordinates"));
    }
}
