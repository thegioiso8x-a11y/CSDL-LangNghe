# CHECKLIST TRƯỚC KHI PROMPT ANTIGRAVITY

Ghi rõ trong prompt các mục sau để tránh AI sinh code sai:

## Bắt buộc

- [ ] Đọc lại file entity/config liên quan — KHÔNG bịa property
- [ ] Handler dùng `request.Dto.Field` — không `request.Field`
- [ ] Exceptions từ `LangNghe.Domain.Exceptions`
- [ ] DbContext: `ILangNgheDbContext` từ `LangNghe.Domain.Interfaces`
- [ ] GetList: `ApiResponse<PagedResult<Dto>>` — GetById: `ApiResponse<Dto>`
- [ ] `PagedResult` constructor: `new PagedResult<T>(items, filter.Page, filter.PageSize, totalCount)`
- [ ] FK config dùng `.WithMany(x => x.Collection)` — KHÔNG share collection giữa 2 navigation
- [ ] Datetime default: `SYSUTCDATETIME()` — KHÔNG `GETDATE()`
- [ ] Async/await + CancellationToken mọi nơi
- [ ] `AsNoTracking()` read — `AsTracking()` mutation
- [ ] File-scoped namespace + XML doc tiếng Việt có dấu
- [ ] Build PHẢI pass 0 error trước khi báo xong

## Nhắc AI khi sinh xong

- [ ] Chạy `dotnet build src/LangNghe.API` và báo kết quả
- [ ] Nếu báo "thành công" → **verify bằng terminal thật** (`ls`, `build`, `grep`)
- [ ] Nếu output bị cắt giữa → yêu cầu sinh lại

## Khi AI lệch scope

Nếu AI tự ý tạo project khác, cài package lạ, hoặc extract từ log chat:

> **DỪNG. Chỉ làm đúng yêu cầu. KHÔNG mở rộng scope.**
