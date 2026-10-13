# CSDL LÀNG NGHỀ — CONTEXT CHO AI

## Mục tiêu
CMS quản lý làng nghề Việt Nam cho Bộ NN&PTNT. Dùng nội bộ. Không app, không portal.

## Tech Stack
- .NET Core 10 (Web API) + EF Core 10 + Dapper + MediatR (CQRS) + FluentValidation + AutoMapper
- SQL Server 2019+ + Redis + Hangfire + Serilog + ClosedXML
- React 18 + TypeScript + Vite + Ant Design 5 + TanStack Query + Recharts
- Docker + IIS

## Phân quyền 3 CẤP (không có Huyện, không có cấp Làng nghề)
- TW (Bộ NN&PTNT) — DataScope=1 — toàn quốc
- Tỉnh (Sở NN&PTNT) — DataScope=2 — trong tỉnh
- Xã/Phường/Đặc khu — DataScope=3 — nhập liệu LN của xã mình
- Làng nghề là ENTITY dữ liệu, KHÔNG phải cấp user

## Ưu tiên P1
1. Auth + Users/Roles
2. Danh mục
3. Làng nghề (Form 6 tab + Copy kỳ trước + Auto-save)
4. Import Excel (3 template × 4 chế độ: MERGE/REPLACE/INSERT/UPDATE)
5. Workflow Xã→Tỉnh→TW
6. Dashboard 3 cấp + Báo cáo 14 chỉ tiêu
7. Export Excel theo template "Đầu ra chi tiết 2" + "Đầu ra tổng"
8. Audit log

## Bảo mật cấp 2
BCrypt password, JWT + Refresh Token, Lockout 5 lần/15p, HTTPS, Audit log, chống SQLi/XSS/CSRF

## Bảng chính (22 bảng)
**Dimension:** Dim_Regions, Dim_RegionMapping, Dim_VillageTypes, Dim_IndustryGroups, Dim_OutputUnits, Dim_ExportCategories, Dim_Classifications
**Master:** Dim_CraftVillages, Dim_Products, Dim_Artisans
**Fact:** Fact_Village_PeriodicMetrics (partition by year + columnstore), Fact_Product_OCOP_History, Fact_Village_Environment, Fact_SubmissionWorkflow
**Aggregated:** Agg_Village_TimeSeries (2 cấp), Agg_By_IndustryGroup, Agg_By_VillageType
**System:** Sys_Users, Sys_Roles, Sys_Permissions, Sys_RolePermissions, Sys_UserRoles, Sys_AuditLog
**Import:** Import_History, Staging_RawImport, Import_Error_Log, Import_TemplateConfig, Fact_Village_Draft, Sys_FormFieldConfig

## Cấu trúc Solution
```
src/
├── LangNghe.Domain/          (Entities, Enums, Interfaces)
├── LangNghe.Infrastructure/  (DbContext, Repos, Migrations)
├── LangNghe.Application/     (CQRS, DTOs, Validators)
├── LangNghe.Import/          (Excel parsers)
├── LangNghe.API/             (Controllers, Middleware)
└── LangNghe.Jobs/            (Hangfire)
tests/
├── LangNghe.UnitTests/
└── LangNghe.IntegrationTests/
```

## Convention
- Entity: PascalCase singular (CraftVillage)
- DTO: `XxxDto` (CreateVillageDto, VillageDetailDto)
- Command/Query + Handler riêng (MediatR)
- Controller mỏng, logic trong Handler
- Async/await + CancellationToken mọi nơi
- `AsNoTracking()` cho read-only query
- Pagination mặc định 20/trang, max 200
- Comment tiếng Việt, code tiếng Anh
- Business exception → BusinessException (400), NotFound (404)

## Response Format
```json
{ "success": true, "data": {...}, "message": "...", "errors": [], "timestamp": "ISO" }
```

## Rules QUAN TRỌNG
1. **Không tự thêm/bớt cột** trong bảng — bám schema đã chốt
2. **Ownership check**: Xã A KHÔNG được nhập/sửa LN của Xã B
3. **Excel linh hoạt**: Chỉ 8 cột required (VillageCode, VillageName, Province, Commune, RecognitionDate, Classification, VillageType, IndustryGroup)
4. **MERGE mode**: Cột không có trong file → giữ nguyên giá trị cũ
5. **Import transaction**: Lỗi > 10% → rollback toàn bộ
6. **Partition + Columnstore** trên Fact — không query trực tiếp Fact khi có thể đọc Agg
7. **Cache 3 tầng**: Memory (10s) → Redis (5p) → DB
8. **Không hardcode** connection string / secret

## Khi nào hỏi lại
- Yêu cầu mâu thuẫn schema → hỏi
- Cần thêm bảng/cột → đề xuất + chờ OK
- Cần package ngoài stack → đề xuất + chờ OK
- Logic nghiệp vụ chưa rõ → hỏi, KHÔNG đoán
