# BÀI HỌC TỪ GIAI ĐOẠN TESTER (2026-10-21)

## 1. DB là nguồn chân lý — Handoff có thể lệch
- Handoff ghi "V1_MINIMAL 8-11 cột" nhưng DB thực tế 11 cột tiếng Việt.
- Handoff ghi "3 template" nhưng DB chỉ có V1_MINIMAL.
- Nguyên tắc: Query DB trước khi sinh file Excel/config/seed data.

## 2. Tên bảng thực tế trong DB (Data Warehouse Pattern)
- Dimension: Dim_CraftVillages, Dim_Regions, Dim_Artisans, Dim_Products, Dim_VillageTypes, Dim_IndustryGroups, Dim_Classifications, Dim_ExportCategories, Dim_OutputUnits, Dim_RegionMapping.
- Fact: Fact_SubmissionWorkflow (16 cột), Fact_Village_PeriodicMetrics (44 cột), Fact_Product_OCOP_History, Fact_Village_Draft, Fact_Village_Environment.
- Aggregation: Agg_Village_TimeSeries, Agg_By_IndustryGroup, Agg_By_VillageType.
- Hệ thống: Sys_Users, Sys_Roles, Sys_Permissions, Sys_RolePermissions, Sys_UserRoles, Sys_RefreshTokens, Sys_AuditLog, Sys_Notifications, Sys_FormFieldConfig.
- Import: Import_History, Import_Error_Log, Import_TemplateConfig, Staging_RawImport.
- Hangfire: schema HangFire.* riêng.

## 3. Dim_CraftVillages KHÔNG có cột IsActive
- Filter làng nghề đang hoạt động dùng cột Status (tinyint): 0=Draft, 1=PendingProvince, 2=ApprovedProvince, 3=Published, 9=Rejected.
- Có cột IsPublished (bit) song song với Status — khi Publish thì cả 2 cùng đổi.

## 4. Dim_Regions dùng LevelType (không phải Level)
- LevelType = 1: Tỉnh/Thành phố (ParentId = NULL).
- LevelType = 2: Xã/Phường (ParentId trỏ về tỉnh mẹ).

## 5. Lỗi "Làng nghề không thuộc xã/phường có thông tin tỉnh hợp lệ"
- Nguyên nhân: Tạo PeriodicMetric cho Village thuộc cấp Tỉnh (Region.ParentId = null).
- Fix: Village phải thuộc Xã (Region.ParentId != null) thì mới tạo được Metric.

## 6. Workflow permission (VERIFIED từ code)
- Submit: DataScope 1/2/3 (phụ thuộc VillageCreate policy), cần có Metric trước.
- Approve: CHỈ DataScope=2 (Tỉnh), status phải là 1.
- Reject: CHỈ DataScope=2 (Tỉnh), status phải là 1, reason bắt buộc.
- Publish: CHỈ DataScope=1 (TW), status phải là 2.
- Lưu ý: Test full flow cần cả admin (submit/publish) + user Tỉnh (approve/reject).

## 7. User test có sẵn trong DB
- admin (Id=1, DataScope=1): password Admin@2026!New.
- so_nn_test (Id=5, DataScope=2, RegionId=1): dùng cho approve/reject.
- xa_test (Id=2, DataScope=3, RegionId=35).
- Script test tự reset password user Tỉnh qua API trước khi login: POST /users/{id}/reset-password.

## 8. CreateMetricDto yêu cầu nhiều field
- Bắt buộc số: revenue, totalLabor, artisanCount, householdCount, enterpriseCount, exportRate, importMaterialRate.
- 13 field bool (nếu không truyền backend default false, không lỗi): brandProtection, hasOCOP3Star, hasWebsite, hasMuseum, hasDocArchive, hasWasteCollection, hasWasteWaterSystem, hasSolidWasteSystem, hasEmissionTreatment, hasDedicatedZone, hasSupportPolicy, hasTourismActivity, hasTourLinkage.

## 9. Tên field DTO — grep trước khi viết script
- Product/Artisan: CraftVillageId (không phải VillageId).
- Metric: CraftVillageId + ReportPeriod + PeriodType.
- Submission: CraftVillageId + ReportPeriod.
- Sai tên → Backend default = 0 → lỗi validation.

## 10. RowVersion gửi dưới dạng rowVersionBase64
- GET /villages/{id} trả về rowVersion (base64).
- PUT /villages/{id} phải gửi tên field rowVersionBase64 — sai tên → lỗi 400 required.

## 11. Swashbuckle 6.6.2 crash với [FromForm] IFormFile
- Triệu chứng: /swagger/v1/swagger.json trả 500.
- Fix: Bọc tất cả [FromForm] vào 1 DTO duy nhất (VD: UploadImportRequest).
- Đã áp dụng: ImportsController, UploadsController, UsersController.

## 12. Lỗi openpyxl (Python) gây crash ClosedXML (C#)
- Triệu chứng: 500 XmlException: Text data of XSD type 'DateTime' was expected.
- Fix: Gán wb.properties.created/modified = datetime(2026,1,1) trước khi save.
- KHÔNG gán None (gây AttributeError: 'NoneType' object has no attribute 'set').

## 13. Backend phải dò Sheet theo header, không lấy Sheet đầu
- Code cũ: Worksheets.FirstOrDefault() → 0 dòng nếu Sheet "Hướng dẫn" đứng trước.
- Fix: Duyệt tất cả Sheet, chọn Sheet khớp nhiều cột với config nhất.
- Áp dụng ở ExcelImportService.ParseAsync + ExcelTemplateService.GetHeadersFromStream.

## 14. Aggregation Rebuild phải tính trực tiếp RegionId=0
- Roll-up qua Tỉnh → nếu dữ liệu Tỉnh lỗi → Toàn quốc null.
- Fix: Tính trực tiếp từ metrics gốc, không qua bảng trung gian.

## 15. Quy tắc verify DB trước khi viết query
- KHÔNG dùng SELECT * FROM [tên đoán] → phải SELECT * FROM sys.tables trước.
- KHÔNG đoán tên cột trong WHERE → phải SELECT TOP 3 * xem cột thực tế trước.
- KHÔNG gộp nhiều query trong 1 batch khi chưa verify schema → 1 câu lỗi hủy cả batch.
- Luôn chạy từng query riêng khi verify schema lần đầu.

## 16. Test Script Python — bắt buộc có timeout + wait_for_api
- Mọi requests.get/post phải có timeout=30.
- Thêm wait_for_api() ping /health tối đa 60s trước login.

## 17. Môi trường
- Ubuntu 24.04: pip cài system-wide bị chặn (PEP 668) → dùng venv (python3 -m venv venv).
- Bash: password chứa ! phải dùng single quote '...' (tránh bash: !@#2026: event not found).
- API: --no-launch-profile cần prefix ASPNETCORE_ENVIRONMENT=Development để load User Secrets.

## 18. Bảo mật (đã fix)
- Secret đã move sang User Secrets, appsettings.json để rỗng.
- Git history clean bằng git filter-repo --replace-text.
- DB dùng user langnghe_app (db_owner), không dùng sa.
