# ⚠️ 15 BUG ĐÃ FIX — TRÁNH LẶP LẠI

| # | Bug | Fix |
|---|---|---|
| 1 | `UserConfiguration.Username` max 50 | → 100 (schema) |
| 2 | `PasswordHash` max 255 | → 500 (schema) |
| 3 | `FullName/Email` max 100 | → 255 |
| 4 | Property `PhoneNumber` | → `Phone` |
| 5 | EF shadow FK `RegionId1` | `.WithMany()` → `.WithMany(x => x.Users)` |
| 6 | EF shadow FK `CraftVillageId1` trên Fact | Xoá collection `VillagePeriodicMetrics` khỏi `ExportCategory` + `OutputUnit` — vì 2 navigation KHÔNG được share 1 collection |
| 7 | `IsEffective` computed SQL trả INT nhưng entity `byte?` | Đổi entity + DTO sang `int?` |
| 8 | `PagedResult<T>` không có constructor | Thêm `(List<T>, page, pageSize, totalCount)` |
| 9 | `GetVillageListQuery` truyền sai thứ tự args | Fix: `(items, filter.Page, filter.PageSize, totalItems)` |
| 10 | Namespace `LangNghe.Application.Exceptions` không tồn tại | Đổi thành `LangNghe.Domain.Exceptions` |
| 11 | Handler truy cập `request.Field` | Đổi thành `request.Dto.Field` |
| 12 | `ApiResponse<XxxDto>.Ok(pagedResult)` | GetList dùng `ApiResponse<PagedResult<XxxDto>>`, GetById dùng `ApiResponse<XxxDto>` |
| 13 | Handler Application dùng `LangNgheDbContext` trực tiếp | Tạo `ILangNgheDbContext` ở Domain, DI đăng ký scoped |
| 14 | `IDateTimeProvider` chưa đăng ký DI | Tạo `DateTimeProvider` + `AddSingleton<IDateTimeProvider, DateTimeProvider>()` |
| 15 | Serilog chỉ ghi file → tưởng API treo | Log ở `src/LangNghe.API/logs/langnghe-YYYYMMDD.log` |
| 16 | `DateTime.Now` trong ArtisanValidators | → `DateTime.UtcNow` |
| 17 | `System.UnauthorizedAccessException` trong Approve/SaveDraft | → `ForbiddenException` / `UnauthorizedException` từ Domain |
| 18 | EF shadow FK `RegionId` trên Import_History | `.WithMany()` → `.WithMany(x => x.ImportHistories)` |
| 19 | EF shadow FK `IndustryGroupId1`/`VillageTypeId1`/`RegionId1` trên Agg | `.WithMany(x => x.Collection)` cho cả 3 |
| 20 | EF shadow FK `ProductId1` trên ProductOcopHistory | `.WithMany(x => x.ProductOcopHistories)` |
| 21 | `SubmitDraftCommand` không tạo SubmissionWorkflow | Tách riêng: Submit API phải gọi `POST /submissions/submit` |
| 22 | 3 SaveChangesAsync rải rác trong Import → partial state | Gộp còn 2 (Failed early + final), rollback tự động |
| 23 | `GetDashboardMetricsQuery` không validate DataScope | Chặn user Tỉnh/Xã xem vùng khác |
| 24 | MapLibre báo lỗi `Worker failed to load` trên Vite | Chuyển sang nhúng thư viện qua CDN (`window.maplibregl`) thay vì import NPM. |
| 25 | Bản đồ xám hoặc xẹp chiều cao 0px trong Ant Form | Gọi `map.resize()` trong `setTimeout` + set `minHeight: 400px` cho container. |
| 26 | White Screen toàn trang do AI tự đổi Named Export thành Default Export | Kiểm tra chéo bằng `grep` giữa file export và file import. |
| 27 | Lỗi AbortError của MapLibre khi React StrictMode unmount | Bọc hàm `map.remove()` trong khối `try/catch`. |

## Nguyên tắc rút ra

1. **KHÔNG share collection** giữa 2 navigation property → EF sinh shadow FK.
2. **Computed column SQL trả INT** — entity phải dùng `int?` không phải `byte?`.
3. **`PagedResult` thứ tự args** phải đúng `(items, page, pageSize, total)`.
4. **Clean Architecture:** Application chỉ dùng interface từ Domain.
5. **Serilog ghi file** — khi debug API, xem `logs/`.
