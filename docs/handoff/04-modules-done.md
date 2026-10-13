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

### ✅ Import Excel (Đợt 7)
- 3 template: V1_FULL (47 cột), V1_MINIMAL (8-11 cột), MONTHLY
- 4 mode: MERGE / REPLACE_FULL / INSERT_ONLY / UPDATE_ONLY
- Endpoints `/api/v1/imports`: upload, preview, list, rollback
- Bảng: Import_History (PK Guid), Staging_RawImport, Import_Error_Log, Import_TemplateConfig
- Rollback > 10% lỗi tự động
- Service trong `LangNghe.Application/Features/Import/` (không tách project)

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
