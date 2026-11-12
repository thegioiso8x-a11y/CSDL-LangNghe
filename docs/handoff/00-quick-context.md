# QUICK CONTEXT — ĐỌC 5 PHÚT

## Dự án là gì
CMS nội bộ quản lý làng nghề VN cho Bộ NN&PTNT.
**3 cấp user:** TW (Bộ) — Tỉnh (Sở) — Xã. KHÔNG có Huyện.

## Stack
.NET 10 + EF Core 10 + MediatR + SQL Server 2022 + React 18 (frontend đã hoàn thiện 100%).

## Đã xong (HOÀN THIỆN 100% — Backend + Frontend + GIS)
- Database 30 bảng
- Auth (JWT + Refresh Token + Captcha + Lockout)
- 6 Danh mục CRUD (VillageType, IndustryGroup, OutputUnit, ExportCategory, Classification, Region)
- Làng nghề CRUD + Ownership check + **Thẻ KPI thống kê (thay đổi theo bộ lọc)**
- Product + Artisan CRUD
- PeriodicMetric (47 chỉ tiêu) + Copy kỳ trước
- Draft + Workflow (Xã → Tỉnh → TW)
- Import Excel (3 template V1_FULL/V1_MINIMAL/MONTHLY + API tải file mẫu 2 Sheet)
- Dashboard + Agg + Báo cáo (Rebuild quét mọi năm + roll-up Tháng/Quý → Năm)
- Audit log + User Management + Hangfire + Notification in-app
- Bản đồ số (GIS): Bản đồ làng nghề, Geocode tự động, Batch geocode job.
- **Frontend (Đợt 10) hoàn tất 100%**: 15+ module pages + **Landing Page** (route `/` public, có nút đăng nhập) + MainLayout + Auth + tất cả CRUD/Dashboard/Import/Audit/User/Notification/GIS.

## Đang làm tiếp
- (Không còn hạng mục lớn dang dở — chờ yêu cầu mới)

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
