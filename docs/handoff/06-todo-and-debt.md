# VIỆC CHƯA LÀM + TECH DEBT

## Đã hoàn tất
- Backend: 10/10 đợt
- Frontend: 10/10 phase (10A-10I)

## Nâng cấp đã note (làm sau khi cần)
- **Hangfire**:
  * Rebuild Agg tự động 2h sáng — thêm project `LangNghe.Jobs` + 1 class gọi `IAggRebuildService.RebuildAllAsync`, 0 sửa code cũ
  * Cleanup `Sys_RefreshTokens` hết hạn hàng ngày
  * Cleanup `Sys_AuditLog` cũ hơn 2 năm (đã có API cleanup thủ công)
- **Cache 3 tầng Redis**: sửa `DashboardCacheService` chèn tầng Redis giữa Memory (10s) và DB. Handlers không đổi.
- **NU1903** (high severity): nâng AutoMapper 13.0.1 và System.Security.Cryptography.Xml 9.0.0
- **CS8602 warnings** (2 chỗ): ExcelImportService.cs dòng 81, VillageOwnershipService.cs dòng 80 — nullable dereference
- **Frontend bundle size 1.5MB**: lazy-load route bằng React.lazy + Suspense (giảm ~40-60%)
- **UnitTests + IntegrationTests**: chưa có
- **API**:
  * Draft/Submission chưa có Notification in-app + Email khi duyệt/từ chối
  * Dashboard chưa có export "Đầu ra chi tiết 2" chi tiết theo làng nghề — đã có summary + detail (2 báo cáo khác)
