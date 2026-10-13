# DATABASE

## Tổng quan
- **30 bảng** (29 schema gốc + `Sys_RefreshTokens`)
- Partition by year + Columnstore index trên `Fact_Village_PeriodicMetrics`
- 34 tỉnh + 3.321 xã/phường/đặc khu
- 7 bảng danh mục đã seed
- 5 roles + 14 permissions + 27 role-permissions
- 4 làng nghề demo: `01001`, `01002`, `06001`, `09001`

## Admin user
| Field | Value |
|---|---|
| Username | `admin` |
| Password hiện tại | **`***REDACTED_ADMIN_PWD***`** |
| DataScope | 1 (TW) |
| Role | `ADMIN_TW` |
| Permissions | 14 (đầy đủ) |

## Bảng bổ sung: Sys_RefreshTokens

Chạy script `docs/migrations/20261008_add_refresh_tokens.sql`:

```
Id           BIGINT IDENTITY PK
UserId       INT FK → Sys_Users (Cascade)
Token        VARCHAR(128) UNIQUE
ExpiresAt    DATETIME
RevokedAt    DATETIME NULL
RevokedByIp  VARCHAR(50) NULL
CreatedAt    DATETIME DEFAULT SYSUTCDATETIME()
CreatedByIp  VARCHAR(50) NULL
ReplacedBy   VARCHAR(128) NULL
Index: IX_RefreshToken_User (UserId, ExpiresAt DESC) INCLUDE (RevokedAt)
```

## Global Query Filter (LangNgheDbContext)

```csharp
builder.Entity<CraftVillage>().HasQueryFilter(x =>
    dataScope == null
 || dataScope == 1
 || (dataScope == 2 && x.Region!.ParentId == userRegionId)
 || (dataScope == 3 && x.RegionId == userRegionId));

builder.Entity<Region>().HasQueryFilter(x => x.IsActive);
builder.Entity<User>().HasQueryFilter(x => x.IsActive);
```

## Cleanup job cần làm

Hangfire job xoá refresh token hết hạn hàng ngày 2h sáng:

```sql
DELETE FROM Sys_RefreshTokens
WHERE ExpiresAt < DATEADD(day, -7, SYSUTCDATETIME())
   OR RevokedAt IS NOT NULL
```
