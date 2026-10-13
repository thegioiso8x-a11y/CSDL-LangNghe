# 🎯 DỰ ÁN: CSDL LÀNG NGHỀ VIỆT NAM

## 📌 TÓM TẮT NHANH
Xây dựng CMS quản lý Cơ sở dữ liệu Làng nghề Việt Nam cho Bộ NN&PTNT.
- **Hệ thống nội bộ**, không public portal, không mobile app
- **~7.800 user** (80 TW + 700 Tỉnh + 7.000 Xã)
- **Bảo mật cấp 2** theo TT 03/2017/TT-BTTTT
- **3 cấp phân quyền**: TW – Tỉnh – Xã (KHÔNG có Huyện, KHÔNG có cấp Làng nghề)

## 🔧 TECH STACK ĐÃ CHỐT

### Backend — .NET 10
- **.NET 10** (TargetFramework = `net10.0`, C# 14)
- **EF Core 10.0.x** + **Dapper 2.1.35**
- **MediatR 12.4.1** (CQRS)
- **FluentValidation 11.3.0**
- **AutoMapper 13.0.1**
- **Hangfire 1.8.17**
- **Serilog 8.0.3**
- **ClosedXML 0.104.2** + **DocumentFormat.OpenXml 3.1.0**
- **BCrypt.Net-Next 4.0.3**
- **JWT** (đã gộp vào shared framework — không cần package)

### Database
- **SQL Server 2022** — Database `CSDL_LangNghe`
- **Connection:** `Server=***REDACTED_DB_HOST***,1433;Database=CSDL_LangNghe;User Id=sa;Password=***REDACTED_SQL_PWD***;TrustServerCertificate=True;MultipleActiveResultSets=True`
- **Redis** (sẽ config sau): `localhost:6379`

### Frontend (làm sau)
- React 18 + TypeScript + Vite + Ant Design 5 + TanStack Query + Recharts

### Môi trường dev
- **OS:** Ubuntu Linux 24.04 (XPS 15 9560)
- **Editor:** VS Code
- **Python 3.12** (đã cài `pymssql`, `openpyxl` qua `--user --break-system-packages`)
- **.NET 10 SDK** (chưa verify — cần kiểm tra `dotnet --version`)
- ⚠️ **Lỗi dpkg:** NVIDIA driver + kernel 7.0.0-34 bị kẹt → **KHÔNG cài được qua apt**, chỉ dùng pip với `--user --break-system-packages`

## 📊 TRẠNG THÁI HIỆN TẠI

### ✅ ĐÃ XONG (100%)

**Database (29 bảng):**
- ✅ Schema SQL đã chạy — 29 bảng trong `CSDL_LangNghe`
- ✅ Collation `Vietnamese_CI_AS`
- ✅ Partition by year + Columnstore index trên `Fact_Village_PeriodicMetrics`

**Dữ liệu đã seed:**
| Bảng | Số lượng |
|---|---|
| Dim_Regions (Tỉnh) | 34 |
| Dim_Regions (Xã) | 3.321 |
| Dim_VillageTypes | 6 |
| Dim_IndustryGroups | 7 |
| Dim_OutputUnits | 15 |
| Dim_ExportCategories | 5 |
| Dim_Classifications | 2 |
| **Dim_CraftVillages** | **4 làng nghề demo** |
| Sys_Roles | 5 |
| Sys_Permissions | 14 |
| Sys_RolePermissions | 27 |

**4 làng nghề demo đã seed:**
- `01001` — Mây tra đan Phú Vinh (Hà Nội > Xã Phú Nghĩa)
- `01002` — Gốm sứ Bát Tràng (Hà Nội > Xã Bát Tràng)
- `06001` — Đúc kim loại Tống Xá (Ninh Bình > Xã Ý Yên)
- `09001` — Tranh đá quý Lục Yên (Lào Cai > Xã Lục Yên)

**Files đã có trên máy:**
```
/media/vuong/01D88ABE649E87C0/
├── Bang Nhap Lieu _V1_20261001.xlsx   # File gốc (dấu cách ở "Liệu")
├── DanhMuc_DVHC.xlsx                   # 34 tỉnh + 3321 xã (tách từ file gốc)
├── 02_seed_regions.sql                 # 272 KB, 3321 INSERT (không dùng nữa)
├── gen_sql.py                          # Sinh file .sql trên
├── import_xa_v3.py                     # Import 3321 xã (đã chạy xong)
├── import_villages_v2.py               # Import 4 làng nghề (đã chạy xong)
└── seed_danhmuc.py                     # Seed 7 bảng danh mục (đã chạy xong)
```

### ⏳ CHƯA LÀM

**Backend .NET 10:**
- ❌ Chưa tạo solution `LangNghe.sln`
- ❌ Chưa có 8 projects (Domain, Infrastructure, Application, Import, API, Jobs, UnitTests, IntegrationTests)
- ❌ Chưa code API/Service/CQRS
- ❌ Chưa có Entity Framework entities
- ❌ Chưa có migration

**Frontend:**
- ❌ Chưa bắt đầu

## 🗂️ SCHEMA DATABASE — 29 BẢNG

### Dimension (8)
- `Dim_Regions` — 2 cấp: LevelType=1 (Tỉnh), LevelType=2 (Xã/Phường/Đặc khu). Có `ParentId` self-ref
- `Dim_RegionMapping` — mapping mã cũ ↔ mới sau sáp nhập 2025
- `Dim_VillageTypes` — 6 loại hình (LNTT, LNM, LNKP, NTT, NNM, NKP)
- `Dim_IndustryGroups` — 7 nhóm ngành (CB, TCMN, XLCB, GOMAY, SVC, MUOI, DV)
- `Dim_OutputUnits` — 15 đơn vị sản lượng
- `Dim_ExportCategories` — 5 mặt hàng TCMN (GO, GOM, THUYTINH, MAYTRE, KHAC)
- `Dim_Classifications` — 2 phân loại (LN, NGHE)

### Master (3)
- `Dim_CraftVillages` — mã 5 số (2 số tỉnh + 3 số TT), FK RegionId → Region (LevelType=2), có RowVersion (optimistic lock)
- `Dim_Products` — sản phẩm + OCOP
- `Dim_Artisans` — nghệ nhân

### Fact (5)
- `Fact_Village_PeriodicMetrics` — 47 chỉ tiêu, partition by year, columnstore
  - Denormalize: `ProvinceId` + `CommuneId` (bỏ JOIN khi query dashboard)
  - Computed: `ExportValueVND`, `IsEffective`
  - ⚠️ **Columnstore KHÔNG được chứa 2 computed columns**
- `Fact_Product_OCOP_History`
- `Fact_Village_Environment`
- `Fact_SubmissionWorkflow` — workflow 2 bước: 0:Draft → 1:PendingProvince → 2:ApprovedProvince → 3:Published / 9:Rejected
- `Fact_Village_Draft` — lưu nháp Form

### Aggregated (3)
- `Agg_Village_TimeSeries` — tổng hợp theo kỳ × vùng (2 cấp)
- `Agg_By_IndustryGroup`
- `Agg_By_VillageType`

### System (7)
- `Sys_Users` — DataScope: 1=TW, 2=Tỉnh, 3=Xã. `RegionId` FK
- `Sys_Roles`, `Sys_Permissions`, `Sys_RolePermissions`, `Sys_UserRoles`
- `Sys_AuditLog` — log mọi CUD
- `Sys_FormFieldConfig`

### Import (4)
- `Import_History`, `Staging_RawImport`, `Import_Error_Log`, `Import_TemplateConfig`

## 🎯 CHỨC NĂNG CHÍNH (ĐÃ CHỐT SCOPE)

### Module 1: Auth + User/RBAC
- Đăng nhập (Username/Password + Captcha) + Lockout 5 lần/15 phút
- JWT + Refresh Token
- Đổi mật khẩu, bắt buộc đổi lần đầu
- CRUD Users (chỉ Tỉnh/TW)
- Bulk-import user từ Excel
- Gán Role/Permission

### Module 2: Danh mục
- CRUD Tỉnh/Xã (chỉ TW)
- CRUD 7 bảng danh mục (chỉ TW)
- **KHÔNG xóa cứng ĐVHC** — chỉ `IsActive = 0` (vì Fact tham chiếu)

### Module 3: Làng nghề (CORE)
- **Form 6-7 tab**: Cơ bản | Sản phẩm-CN | Quy mô-DT | Nhân lực | CDS | Hạ tầng-MT | Du lịch
- Auto-save draft mỗi 30 giây
- **Copy kỳ trước** — auto-fill 90% giá trị
- **Ownership check**: User cấp Xã chỉ nhập LN của xã mình (2 số đầu mã LN phải khớp mã tỉnh user)
- **KHÔNG bắt buộc đủ 47 cột** — chỉ 8 cột required

### Module 4: Import Excel
- **3 template**: `V1_FULL` (47 cột), `V1_MINIMAL` (8 cột), `MONTHLY` (7 cột)
- **4 chế độ**: MERGE (khuyến nghị) / REPLACE_FULL / INSERT_ONLY / UPDATE_ONLY
- **MERGE mode:** chỉ update cột có trong file, giữ nguyên cột khác
- Parser đọc **header 3 tầng động** (không hardcode cột theo index)
- Validation 2 phase: Structural → Business
- Transaction: lỗi > 10% → rollback
- Preview trước khi commit
- Rollback theo `ImportBatchId`

### Module 5: Workflow 2 bước
- Xã submit → Tỉnh duyệt/trả lại → TW công bố
- Trạng thái lưu ở `Fact_SubmissionWorkflow`
- Notification in-app + Email

### Module 6: Báo cáo + Dashboard
- Dashboard 3 cấp (TW/Tỉnh/Xã)
- 9 KPI cards + 4 biểu đồ
- 14 chỉ tiêu giám sát (đọc từ view `vw_Report_ChiTieu_*`)
- Export Excel theo template "Đầu ra chi tiết 2" + "Đầu ra tổng"
- **Cache 3 tầng:** Memory (10s) → Redis (5p) → DB
- **Đọc từ `Agg_*`** tables, KHÔNG query Fact

### Module 7: Audit
- Log mọi CUD + IP + User-Agent
- Lưu OldValues/NewValues (JSON)
- Retention 2 năm

## 🎨 CONVENTION CODE

### Naming
- Entity: `CraftVillage` (PascalCase, singular)
- DTO: `CreateVillageDto`, `VillageDetailDto`
- Command: `CreateVillageCommand` + `CreateVillageCommandHandler`
- Query: `GetVillagesQuery` + `GetVillagesQueryHandler`
- Service: `ExcelImportService` + `IExcelImportService`
- Private field: `_camelCase`
- Async: hậu tố `Async` + `CancellationToken`

### Ngôn ngữ
- **Code + identifiers:** Tiếng Anh
- **Comment + XML doc:** Tiếng Việt có dấu
- **Message lỗi trả user:** Tiếng Việt có dấu

### Response Format chuẩn
```json
{
  "success": true,
  "data": {},
  "message": "Thao tác thành công",
  "errors": [],
  "timestamp": "2026-10-08T10:30:00Z"
}
```

### Best Practices .NET 10
- `async/await` + `CancellationToken` mọi nơi
- `AsNoTracking()` cho read-only
- Pagination mặc định 20/trang, max 200
- Projection (`.Select()`) thay vì load full entity
- Dùng `record` cho DTO
- Dùng **Primary Constructor** (C# 12+)
- MediatR 12.x: `cfg.RegisterServicesFromAssembly(assembly)` (không `AddMediatR(typeof(...))`)
- OpenAPI built-in thay Swashbuckle: `AddOpenApi()` + `MapOpenApi()`
- **KHÔNG cần** package `Microsoft.AspNetCore.Authentication.JwtBearer`

### Error Handling
| Exception | HTTP |
|---|---|
| `BusinessException` | 400 |
| `NotFoundException` | 404 |
| `UnauthorizedException` | 401 |
| `ForbiddenException` | 403 |
| `ValidationException` | 422 |
| `Exception` | 500 |

## 🚫 NHỮNG ĐIỀU ĐÃ LOẠI BỎ (KHÔNG LÀM LẠI)

- ❌ Cấp Huyện trong phân quyền
- ❌ Cấp Làng nghề trong phân quyền
- ❌ Mobile app
- ❌ Public portal
- ❌ Multi-language (VI/EN) — chỉ tiếng Việt
- ❌ Dark mode
- ❌ SSO VNeID (chưa cần)
- ❌ Push notification
- ❌ SMS notification
- ❌ Bản đồ số GIS (làm sau — P3)
- ❌ 2FA qua SMS
- ❌ Full-Text Search (chưa cài SQL Server feature) — dùng `LIKE` thay `CONTAINS`

## 📋 LỘ TRÌNH ĐỀ XUẤT (CHIA NHỎ)

### Giai đoạn 1: Setup .NET 10 (3-4 giờ)
- Task 1A: Solution + 8 csproj + Directory.Build.props + global.json
- Task 1B: Program.cs + appsettings + Middleware + HealthController
- Task 1C: 4 file DependencyInjection.cs

### Giai đoạn 2: Domain Model (2-3 giờ)
- Task 2A: 7 Entity danh mục nhỏ
- Task 2B: 3 Entity master
- Task 2C: 5 Entity Fact
- Task 2D: 13 Entity còn lại
- Task 2E: DbContext + Configurations
- Task 2F: Base classes (UnitOfWork, ICurrentUserService)

### Giai đoạn 3: Auth (2 giờ)
- Task 3A: JWT + Password Service
- Task 3B: Login/Logout/Refresh
- Task 3C: User CRUD + Lock/Unlock

### Giai đoạn 4: Villages CRUD (3 giờ)
- Task 4A: CRUD cơ bản
- Task 4B: Draft + Copy kỳ trước
- Task 4C: Workflow

### Giai đoạn 5: Import Excel (4-5 giờ)
- Task 5A: Excel Parser (header 3 tầng)
- Task 5B: Validation Service
- Task 5C: Preview Service
- Task 5D: Persist Service (4 chế độ)
- Task 5E: Import Controller + Rollback

### Giai đoạn 6: Report (3 giờ)
- Task 6A: Dashboard + KPI
- Task 6B: 14 chỉ tiêu
- Task 6C: Export Excel + Background Job

### Giai đoạn 7: Test + Deploy (4 giờ)
- Task 7A: Unit Test Framework
- Task 7B: Integration Test Framework
- Task 7C: Docker Compose
- Task 7D: appsettings.Production + HTTPS

**Tổng: ~20-25 giờ** cho MVP backend.

## 🔍 CÂU HỎI THƯỜNG GẶP KHI TIẾP TỤC

### Q: Sao không dùng migration để tạo bảng?
**A:** Database đã có 29 bảng (chạy script SQL). Cần **reverse engineering** từ DB → Entities, KHÔNG chạy `dotnet ef migrations add` (tránh conflict).

### Q: Sao không tách File Excel thành file riêng?
**A:** Đã tách — `DanhMuc_DVHC.xlsx` (34 tỉnh + 3321 xã). File gốc giữ nguyên 3 sheet (NHẬP LIỆU + Bảng phụ + DANH MỤC).

### Q: Có cần dùng Dapper không khi đã có EF Core?
**A:** Có — Dapper cho read-heavy query (Report, Dashboard, Ranking). EF Core cho write + CRUD. Đừng dùng lẫn lộn.

### Q: Ownership check hoạt động thế nào?
**A:** User cấp Xã chỉ nhập LN của xã mình. Check: 2 số đầu của `VillageCode` phải khớp mã tỉnh của user. VD: User ở Hà Nội (mã 01) chỉ nhập LN `01xxx`.

### Q: MERGE mode import khác gì REPLACE?
**A:** 
- MERGE: Chỉ update cột **có trong file**, giữ nguyên cột khác (khuyến nghị)
- REPLACE_FULL: Update toàn bộ, cột thiếu → set NULL (nguy hiểm)

### Q: Tại sao DataScope là 1/2/3 mà không phải 0/1/2?
**A:** Để dễ đọc code: `1 = National`, `2 = Province`, `3 = Commune` (giống hierarchy level).

## ⚠️ LƯU Ý QUAN TRỌNG

1. **Password SQL Server:** `***REDACTED_SQL_PWD***` — có ký tự đặc biệt, cần escape khi dùng trong bash. Dùng `getpass` hoặc env var.

2. **File Excel gốc có dấu cách:** `Bang Nhap Lieu _V1_20261001.xlsx` (space ở "Liệu "). Khi đọc phải dùng đúng tên.

3. **Header Excel 3 tầng:**
   - Row 1: Nhóm lớn
   - Row 2: Chỉ tiêu con
   - Row 3: Mã trường (-1, -2, ..., -47)
   - Row 4: (Trống hoặc sub-header)
   - Row 5: Mã số tiếp (-1 đến -47) **← Đây là row mã trường**
   - **Row 6-9:** 4 làng nghề demo (data thật)

4. **Database hiện tại:**
   - `Sys_Users` **CHƯA có admin** (chưa seed vì password hash BCrypt chưa generate)
   - `Sys_UserRoles` rỗng
   - `Fact_*` tables rỗng

5. **Lỗi dpkg không fix được** — đừng cố `apt install`, dùng pip với `--user --break-system-packages`.

## 🎯 VIỆC CẦN LÀM NGAY (KHI TIẾP TỤC)

**Câu hỏi đầu tiên cho AI trong chat mới:**

> "Tôi đang làm dự án CSDL Làng nghề Việt Nam. Đã có database 29 bảng + 34 tỉnh + 3321 xã + 4 làng nghề demo. Cần tiếp tục:
> 
> 1. Setup solution .NET 10 với 8 projects
> 2. Generate 29 Entity classes khớp schema DB hiện tại (KHÔNG chạy migration)
> 3. Code Auth + CRUD Làng nghề + Import Excel + Dashboard
> 
> Tech stack: .NET 10, EF Core 10, MediatR 12, Dapper 2.1, Hangfire 1.8.
> 
> Bắt đầu bằng việc xác nhận hiểu yêu cầu + đề xuất cấu trúc 8 projects."

## 📞 THÔNG TIN BỔ SUNG

- **Ngôn ngữ với user:** Tiếng Việt có dấu
- **Style phản hồi:** Ngắn gọn, có ví dụ code cụ thể, không lan man
- **Khi có lỗi:** Hỏi lại nếu mơ hồ, KHÔNG tự đoán
- **Deliverable:** File hoàn chỉnh, không chia "phần 1/3"
## 🔐 THÔNG TIN NHẠY CẢM (KHÔNG CHIA SẺ RA NGOÀI)

### SQL Server
- Server: ***REDACTED_DB_HOST***,1433
- User: sa
- Password: ***REDACTED_SQL_PWD***
- Database: CSDL_LangNghe

### Trạng thái DB hiện tại
- 29 bảng đã tạo
- 34 tỉnh + 3321 xã
- 7 bảng danh mục (6+7+15+5+2 = 35 records)
- 4 làng nghề demo (01001, 01002, 06001, 09001)
- 5 roles + 14 permissions + 27 role-permissions

### Files quan trọng
- `DanhMuc_DVHC.xlsx` — nguồn dữ liệu chính
- `01_schema_v3.sql` — schema chuẩn cuối cùng
- `import_*.py` — scripts đã chạy
- `handoff.md` — context dự án
- `Agents.md` — config cho AI editor
---

**END OF HANDOFF DOCUMENT**
