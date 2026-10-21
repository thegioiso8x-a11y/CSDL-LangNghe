# BÀI HỌC TỪ GIAI ĐOẠN TESTER (2026-10-21)

## 1. DB là nguồn chân lý — Handoff có thể lệch
- Handoff ghi "V1_MINIMAL 8-11 cột" nhưng DB thực tế **11 cột tiếng Việt**.
- Handoff ghi "3 template" nhưng DB **chỉ có V1_MINIMAL**.
- **Nguyên tắc:** Query DB trước khi sinh file Excel/config/seed data.

## 2. Phân biệt Code (string) và Id (int)
- `RegionCode` = "01" (string) ≠ `RegionId` = 1761 (int).
- `VillageTypeCode` = "LNTT" (string) ≠ `VillageTypeId` = 1 (int).
- Import Excel dùng **INT**, không phải Code.

## 3. Lỗi openpyxl (Python) gây crash ClosedXML (C#)
- **Triệu chứng:** 500 `XmlException: Text data of XSD type 'DateTime' was expected`.
- **Fix:** Gán `wb.properties.created/modified = datetime(2026,1,1)` trước khi save.
- **Không** gán `None` (gây `AttributeError: 'NoneType' object has no attribute 'set'`).

## 4. Backend phải dò Sheet theo header, không lấy Sheet đầu
- Code cũ: `Worksheets.FirstOrDefault()` → 0 dòng nếu Sheet "Hướng dẫn" đứng trước.
- **Fix:** Duyệt tất cả Sheet, chọn Sheet khớp nhiều cột với config nhất.
- Áp dụng ở `ExcelImportService.ParseAsync` + `ExcelTemplateService.GetHeadersFromStream`.

## 5. Tên field DTO — grep trước khi viết script
- Product/Artisan: `CraftVillageId` (không phải `VillageId`).
- Metric: `CraftVillageId` + `ReportPeriod` + `PeriodType` (không phải `villageId`/`period`).
- Sai tên → Backend default = 0 → lỗi "ID làng nghề không hợp lệ".

## 6. RowVersion gửi dưới dạng `rowVersionBase64`
- `GET` trả `rowVersion` (base64).
- `PUT` phải gửi tên field **`rowVersionBase64`** — sai tên → lỗi 400 required.

## 7. Swashbuckle 6.6.2 crash với `[FromForm] IFormFile`
- **Triệu chứng:** `/swagger/v1/swagger.json` trả 500.
- **Fix:** Bọc tất cả `[FromForm]` vào 1 DTO duy nhất (VD: `UploadImportRequest`).
- Áp dụng: `ImportsController`, `UploadsController`, `UsersController`.

## 8. Aggregation Rebuild phải tính trực tiếp RegionId=0
- Roll-up qua Tỉnh → nếu dữ liệu Tỉnh lỗi → Toàn quốc null.
- **Fix:** Tính trực tiếp từ `metrics` gốc, không qua bảng trung gian.

## 9. Test Script Python — bắt buộc có timeout
- Mọi `requests.get/post` phải có `timeout=30`.
- Thêm `wait_for_api()` ping `/health` tối đa 60s trước login.

## 10. Môi trường
- **Ubuntu 24.04:** pip cài system-wide bị chặn (PEP 668) → dùng venv (`python3 -m venv venv`).
- **Bash:** password chứa `!` phải dùng single quote `'...'` (tránh `bash: !@#2026: event not found`).
- **API:** `--no-launch-profile` cần prefix `ASPNETCORE_ENVIRONMENT=Development` để load User Secrets.

## 11. Bảo mật (đã fix)
- Secret đã move sang User Secrets, `appsettings.json` để rỗng.
- Git history clean bằng `git filter-repo --replace-text`.
- DB dùng user `langnghe_app` (db_owner), không dùng `sa`.
