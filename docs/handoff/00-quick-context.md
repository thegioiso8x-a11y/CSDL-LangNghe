# QUICK CONTEXT — ĐỌC 5 PHÚT

## Dự án là gì
CMS nội bộ quản lý làng nghề VN cho Bộ NN&PTNT.
**3 cấp user:** TW (Bộ) — Tỉnh (Sở) — Xã. KHÔNG có Huyện.

## Stack
.NET 10 + EF Core 10 + MediatR + SQL Server 2022 + React 18 (chưa làm frontend).

## Đã xong
- Database 30 bảng
- Auth (JWT + Refresh Token + Captcha + Lockout)
- 6 Danh mục CRUD (VillageType, IndustryGroup, OutputUnit, ExportCategory, Classification, Region)
- Làng nghề CRUD + Ownership check
- Product + Artisan CRUD
- PeriodicMetric (47 chỉ tiêu) + Copy kỳ trước
- Bản đồ số (GIS): Bản đồ làng nghề, Geocode tự động, Batch geocode job.

## Đang làm tiếp
- 6D: Draft form + Workflow submit (Xã → Tỉnh → TW)
- 7: Import Excel
- 8: Dashboard + Agg + Báo cáo
- 9: Audit log
- 10: Frontend

## Quy tắc tối quan trọng
1. Handler dùng `request.Dto.Field` (không `request.Field`)
2. Exceptions: `LangNghe.Domain.Exceptions`
3. DbContext: `ILangNgheDbContext` (Domain.Interfaces)
4. GetList: `ApiResponse<PagedResult<Dto>>` — GetById: `ApiResponse<Dto>`
5. `PagedResult(items, page, pageSize, totalCount)` — ĐÚNG THỨ TỰ
6. FK config: `.WithMany(x => x.Collection)` — KHÔNG share collection
7. Datetime default: `SYSUTCDATETIME()` — KHÔNG `GETDATE()`
8. Build PHẢI pass 0 error trước khi báo xong

## Login test
username: admin
password: ***REDACTED_ADMIN_PWD*** ← đã đổi từ ***REDACTED_ADMIN_PWD***
