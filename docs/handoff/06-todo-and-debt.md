# VIỆC CHƯA LÀM + TECH DEBT

## Việc chưa làm
- **Đợt 10:** Frontend React 18 + Vite + AntD 5 + TanStack Query + RHF/Zod + Recharts + Zustand

## Nâng cấp đã note (làm sau)
- **Hangfire rebuild Agg tự động** 2h sáng: chỉ cần thêm project LangNghe.Jobs + 1 class gọi lại `AggRebuildService.RebuildAllAsync` — 0 sửa code cũ
- **Cache 3 tầng Redis**: chỉ sửa `DashboardCacheService`, không đụng handler
- **NU1903 fix**: AutoMapper 13.0.1 → nâng version mới; System.Security.Cryptography.Xml 9.0.0 → nâng
- **Hangfire cleanup refresh token** hàng ngày
- **UnitTests + IntegrationTests**: chờ frontend xong
