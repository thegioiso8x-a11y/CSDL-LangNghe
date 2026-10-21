# MODULE ĐÃ HOÀN THÀNH

## ✅ Auth (100%)

Route `/api/v1/auth`:

| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/captcha` | Math captcha (a+b hoặc a×b), cache 5 phút |
| POST | `/login` | username/password/captcha → JWT + RefreshToken |
| POST | `/refresh` | Rotate refresh token |
| POST | `/revoke` | Thu hồi token |
| POST | `/change-password` | [Authorize] + revoke all refresh tokens |

Lockout: 5 lần sai / 15 phút.

**JWT claims:** `sub` (UserId), `ClaimTypes.Name` (Username), `dataScope`, `regionId`, `role[]`, `permission[]`.

**Config appsettings.json:**

```json
"Jwt": {
  "SecretKey": "***REDACTED_JWT_KEY***",
  "Issuer": "LangNghe.API",
  "Audience": "LangNghe.Client",
  "AccessTokenMinutes": 60,
  "RefreshTokenDays": 7
}
```

## ✅ 6 Danh mục

| Module | Route | Đặc điểm |
|---|---|---|
| VillageType | `/api/v1/village-types` | Soft delete |
| IndustryGroup | `/api/v1/industry-groups` | Soft delete |
| OutputUnit | `/api/v1/output-units` | Soft, KHÔNG có CreatedAt |
| ExportCategory | `/api/v1/export-categories` | Soft, KHÔNG có CreatedAt |
| Classification | `/api/v1/classifications` | **Hard delete** + check FK |
| Region | `/api/v1/regions` | 2 cấp Tỉnh/Xã, self-ref, `GET /tree` |

## ✅ Village

Route `/api/v1/villages`:

- `GET /` — phân trang, filter đa tiêu chí
- `GET /{id:int}` và `GET /by-code/{code}`
- `POST /` [VillageCreate] — Ownership check
- `PUT /{id:int}` [VillageUpdate] — Optimistic lock qua RowVersion
- `DELETE /{id:int}` [VillageDelete] — Hard delete + check con
- `POST /{id:int}/publish?publish=` [VillagePublish]

**VillageOwnershipService:**

- `ValidateCreateAsync(regionId, villageCode, ct)` — TW: OK; Tỉnh: region.ParentId == userRegionId + 2 số đầu code khớp; Xã: regionId == userRegionId + khớp tỉnh mẹ
- `ValidateVillageAccessAsync(villageId, ct)`
- `ValidateMetricAccessAsync(villageId, ct)` — wrapper

## ✅ Product + Artisan

- `/api/v1/products` — OCOP validate (HasOCOP=true → OcopStar ∈ {3,4,5})
- `/api/v1/artisans` — Phone regex, birth/recognition year range

## ✅ PeriodicMetric (47 chỉ tiêu)

Route `/api/v1/periodic-metrics` (**chú ý Id là `long`**):

- `GET /` — filter theo Village/Province/Commune/Period/Type/IsEffective/Year
- `GET /{id:long}`
- `GET /by-village/{villageId:int}`
- `POST /` [VillageCreate] — auto-fill Commune/Province từ Village
- `PUT /{id:long}` [VillageUpdate]
- `DELETE /{id:long}` [VillageDelete]
- `POST /copy-previous` [VillageCreate] — clone 33 chỉ tiêu từ kỳ gần nhất

**Validators:**

- ExportRate + ImportMaterialRate ≤ 100
- ArtisanCount ≤ TotalLabor
- GrowthRate ∈ [-100, 100]
- ProductionProcess ∈ {Truyền thống, Hiện đại, Kết hợp}

### ✅ Import Excel (Đợt 7) — VERIFIED TỪ DB 2026-10-21

**⚠️ Sự thật từ DB (khác tài liệu cũ):**
- Database **CHỈ có 1 template active: V1_MINIMAL** (11 cột tiếng Việt không dấu).
- V1_FULL và MONTHLY **chưa được seed** vào bảng `Import_TemplateConfig`.
- Tài liệu cũ ghi "3 template: V1_FULL 47 cột, V1_MINIMAL 8-11 cột, MONTHLY" là **sai/thiếu** — phải verify DB.

**Cấu trúc V1_MINIMAL (11 cột, từ `Import_TemplateConfig`):**

| # | Header | FieldName | Type | Req |
|---|---|---|---|---|
| 1 | Ma lang nghe | VillageCode | string | ✅ |
| 2 | Ten lang nghe | Name | string | ✅ |
| 3 | Ma xa | **RegionId** | **int** | ✅ |
| 4 | Ngay cong nhan | RecognitionDate | datetime | ✅ |
| 5 | Quyet dinh | RecognitionDecision | string | ✅ |
| 6 | Phan loai | **ClassificationId** | **int** | ✅ |
| 7 | Loai hinh | **VillageTypeId** | **int** | ✅ |
| 8 | Nhom nganh | **IndustryGroupId** | **int** | ✅ |
| 9 | Dia chi | Address | string | ⬜ |
| 10 | Can phuc hoi | NeedRestoration | bool | ⬜ |
| 11 | Trang thai | Status | int | ⬜ |

**Quan trọng:** Các cột `RegionId/ClassificationId/VillageTypeId/IndustryGroupId` là **INT** (ID nội bộ VD: 1, 8, 35) — KHÔNG phải mã string ("01", "LNTT").

**Chế độ:** MERGE / REPLACE_FULL / INSERT_ONLY / UPDATE_ONLY.

**Endpoints `/api/v1/imports`:**
- `POST /upload` — multipart (file, templateCode, importMode)
- `GET /{batchId:guid}/preview` — chi tiết lỗi
- `GET /` — lịch sử (phân trang)
- `POST /{batchId:guid}/rollback` — đánh dấu rollback

**Rollback tự động:** > 10% dòng lỗi.

**Lưu ý sinh file Excel (openpyxl):**
- Set `wb.properties.created/modified = datetime(2026,1,1)` — KHÔNG gán `None` (gây crash ClosedXML).
- Backend dò Sheet có header khớp config (không lấy Sheet đầu tiên).
### ✅ Dashboard + Agg (Đợt 8)
- Agg rebuild THỦ CÔNG qua `POST /api/v1/aggregation/rebuild` (Hangfire để sau)
- 3 bảng Agg: VillageTimeSeries, IndustryGroupAggregate, VillageTypeAggregate
- Dashboard: 14 chỉ tiêu (từ sheet "Đầu ra tổng")
- Cache 2 tầng: Memory 10s → DB (Redis để sau)
- Export 2 báo cáo Excel: "Đầu ra tổng" + "Đầu ra chi tiết 2" (ClosedXML)
- DataScope validation: user Tỉnh/Xã chỉ xem vùng mình

### ✅ Audit Log (Đợt 9)
- `AuditLogInterceptor` (SaveChangesInterceptor) + `AuditableEntityInterceptor` cũ
- Ghi CUD + IP + User-Agent + OldValues/NewValues (JSON diff)
- Bỏ qua: AuditLog, Agg tables, Import tables, RefreshToken
- Endpoints `/api/v1/audit-logs`: list, get by id, cleanup (retention ≥ 1)
- Retention 2 năm mặc định
- IHttpContextInfo (Application interface) + HttpContextInfo (Infrastructure impl)

### ✅ Draft + Workflow (Đợt 6D)
- Draft: SaveDraft, SubmitDraft (chỉ flip status — KHÔNG tạo workflow)
- Workflow: Submit riêng qua `POST /api/v1/submissions/submit` (đã có metric kỳ đó)
- Approve (Tỉnh), Reject (Tỉnh), Publish (TW)
- Status: 0 Draft → 1 PendingProvince → 2 ApprovedProvince → 3 Published / 9 Rejected
### ✅ Frontend (Đợt 10 hoàn tất)

Stack: React 18 + TypeScript strict + Vite + AntD 5 + TanStack Query v5 + RHF/Zod + Zustand + Recharts + dayjs

Cấu trúc `frontend/src/`:
- `api/` — axios instance + 11 API modules (auth, villages, products, artisans, periodicMetrics, workflow, imports, dashboard, auditLogs + shared)
- `components/layout/` — MainLayout (Sider + Header + Dropdown user), ProtectedRoute
- `components/common/` — DataTable, DeleteConfirm, PermissionGate
- `contexts/AuthContext.tsx` — user + token + hasPermission/hasRole/isDataScope
- `hooks/` — useCRUD, useApiError
- `utils/` — storage (localStorage token), format (date/number/currency)
- `pages/` — 15 module pages

Module đã xong:
- 10A: Setup + Vite proxy + Auth (Login/Logout/Refresh token) + MainLayout
- 10B: Profile + đổi mật khẩu + PermissionGate + useApiError
- 10C: 6 Danh mục CRUD (village-types, industry-groups, output-units, export-categories, classifications, regions)
- 10D: Village (dependent dropdown tỉnh/xã, RowVersion) + Product (OCOP validate) + Artisan (Phone/SĐT regex, năm hợp lệ)
- 10E: PeriodicMetric form 7 tab + Copy kỳ trước + Detail modal
- 10F: Draft (auto-save 30s, JSON editor) + Workflow (List/Pending/Detail với action theo DataScope + Status)
- 10G: Import Excel (Upload Dragger, 4 mode, Preview batch, Rollback) — CHỈ frontend, backend đã có
- 10H: Dashboard (9 KPI card + 4 Recharts + FilterBar + RebuildButton + Export Excel) — đã hỗ trợ "Toàn quốc" RegionId=0
- 10I: Audit Log (List + filter + Detail diff JSON + Cleanup)

Backend bổ sung trong Đợt 10:
- Fix permission policy: 9 policy mới (VillageRead, VillageApprove, ImportUpload, ImportRollback, ReportView, ReportExport, UserManage, AuditView, ConfigManage) + gắn vào 23 endpoints của 5 Controller
- Fix Aggregation Toàn quốc: bỏ FK `FK_VillageTimeSeries_Region` + CHECK constraint mở `RegionLevel IN (0,1,2)` + roll-up trong `AggRebuildService.RebuildTimeSeriesAsync` với weighted average cho AvgIncome/AvgGrowthRate
- `VillageTimeSeriesConfiguration` + `RegionConfiguration` bỏ navigation giữa Agg ↔ Region

### ✅ User Management (UM-1/2/3)

Backend `Features/User/`:
- Queries: GetUserListQuery, GetUserByIdQuery, GetRolesQuery, GetPermissionsQuery
- Commands: CreateUserCommand, UpdateUserCommand, DeleteUserCommand (soft), ResetPasswordCommand, AssignRolesCommand
- Services: UserImportService (INSERT_ONLY / UPDATE_ONLY / UPSERT, preview + import + template)
- Validators: username regex, password, phone VN, DataScope
- Controller: `/api/v1/users` — list/get/roles/permissions/create/update/delete/reset-password/assign-roles/import/preview/import/template
- Policy: UserManage

Frontend `/users`:
- List + filter (keyword, dataScope, region, isActive, roleCode)
- Form tạo/sửa (dependent RegionId khi DataScope != 1)
- ResetPasswordModal
- Import Modal (upload → preview → import + tải template)
- Menu "Người dùng" chỉ hiện nếu user có USER.MANAGE

Đặc điểm:
- Import Excel 8 cột: Username, Password, FullName, Email, Phone, DataScope, RegionCode, RoleCodes
- Preview trước khi import, rollback > 10% lỗi
- Sync UserRoles khi update
- Soft delete (IsActive=false) — không xóa record
- Reset password tự revoke toàn bộ refresh token

### ✅ Hangfire Automation

Project mới `src/LangNghe.Jobs/`:
- 3 recurring jobs:
  * `rebuild-agg-daily` — 2h sáng UTC, gọi `IAggRebuildService.RebuildAllAsync`
  * `cleanup-token-daily` — 3h sáng, xóa refresh token hết hạn > 7 ngày hoặc đã revoke
  * `cleanup-audit-daily` — 4h sáng, xóa audit log cũ > 2 năm
- DependencyInjection extension: `AddHangfireJobs(connStr)` + `ScheduleHangfireJobs()`

API integration:
- `Program.cs` — đăng ký Hangfire + schedule jobs khi app start
- Dashboard `/hangfire` — filter `HangfireDashboardAuthFilter`: Dev cho phép tất cả, Production yêu cầu DataScope=1 (TW)
- Connection dùng `DefaultConnection`

### ✅ Notification in-app (NOT-1/2/4)

**Schema:**
- Bảng `Sys_Notifications` (Id BIGINT, UserId, Title, Message, Type, EntityType, EntityId, LinkUrl, IsRead, ReadAt, CreatedAt)
- Index `IX_Notification_User_Read` (UserId, IsRead, CreatedAt DESC)
- File migration: `docs/migrations/20261014_add_notifications.sql`
- Apply qua endpoint tạm `POST /api/v1/dev/migration/notifications` (chỉ TW)

**Backend `Features/Notifications/`:**
- Entity: `Notification`
- Interface: `INotificationService` (CreateAsync, CreateBulkAsync, CountUnreadAsync)
- Service: `NotificationService` — Add vào context, KHÔNG tự SaveChanges
- Constants: `NotificationTypes` (WorkflowSubmit/Approve/Reject/Publish/System)
- Queries: GetMyNotificationsQuery, GetUnreadCountQuery
- Commands: MarkAsReadCommand, MarkAllAsReadCommand
- Controller: `/api/v1/notifications` — GET list, GET unread-count, POST read, POST read-all
- Chỉ `[Authorize]` (mọi user xem được thông báo của mình)

**Workflow triggers (4 handler):**
- SubmitWorkflowCommand → notify user Tỉnh cùng tỉnh với làng nghề
- ApproveWorkflowCommand → notify người tạo workflow + toàn bộ user TW
- RejectWorkflowCommand → notify người tạo workflow (kèm lý do)
- PublishWorkflowCommand → notify người tạo workflow
- Notification lưu bằng SaveChangesAsync lần 2 (sau nghiệp vụ chính)

**Frontend:**
- `NotificationBell.tsx` — Bell icon + Badge count, polling 30s, dropdown 5 notification mới nhất
- `pages/notifications/List.tsx` — trang danh sách đầy đủ + filter + pagination
- Route `/notifications`
- Tích hợp Bell vào MainLayout (trước dropdown user)

### ✅ GIS (Bản đồ số) — Hoàn tất 100%

- **GIS-1+2**: Entity có Latitude/Longitude; `NominatimGeocodingService`; API `POST /villages/{id}/geocode` và `POST /villages/geocode-address`; Rate limit 1.1s.
- **GIS-3**: Trang `/map` hiển thị bản đồ toàn cảnh VN (center 106.299, 16.0, zoom 4.5), filter Tỉnh/Loại hình/Keyword, marker đỏ, popup thông tin.
- **GIS-4**: `LocationPicker` trong Form Tạo/Sửa làng nghề; Nút "Tìm tọa độ tự động"; `BatchGeocodeJob` chạy Hangfire 1h sáng hàng ngày.
- **Kiến trúc Frontend MapLibre**: Nhúng CDN vào `index.html`, dùng biến toàn cục `window.maplibregl` để triệt tiêu lỗi Vite Worker/White Screen. Ép `map.resize()` trong `setTimeout` để chống xẹp chiều cao trong Ant Form.
