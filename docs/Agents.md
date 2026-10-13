# AGENTS.md — CSDL LÀNG NGHỀ VIỆT NAM

> **File này là "nguồn chân lý duy nhất" (Single Source of Truth) cho mọi AI coding assistant.**
> Đọc kỹ trước khi generate bất kỳ dòng code nào.

---

## 1. TỔNG QUAN DỰ ÁN

**Mục tiêu:** Xây dựng CMS (Content Management System) quản lý Cơ sở dữ liệu Làng nghề Việt Nam phục vụ Bộ Nông nghiệp & Phát triển Nông thôn (NN&PTNT).

**Đặc điểm:**
- Hệ thống **nội bộ**, KHÔNG có public portal, KHÔNG có mobile app
- **Phân cấp 3 cấp:** TW (Bộ NN&PTNT) – Tỉnh (Sở NN&PTNT) – Xã/Phường/Đặc khu (Cán bộ NN)
- **KHÔNG có cấp Huyện, KHÔNG có cấp Làng nghề** trong phân quyền
- Làng nghề chỉ là **entity dữ liệu** do cấp Xã quản lý
- Quy mô: ~7.800 user, 34 tỉnh, ~3.321 xã, ~10.000 làng nghề
- Bảo mật **cấp 2** theo TT 03/2017/TT-BTTTT

---

## 2. TECH STACK (KHÔNG TỰ Ý THAY ĐỔI)

### Backend
- **.NET Core 10** — Web API
- **Entity Framework Core 10** — ORM cho write + CRUD
- **Dapper** — raw SQL cho read-heavy query + report
- **MediatR** — CQRS pattern (Commands / Queries / Handlers)
- **FluentValidation** — validation layer
- **AutoMapper** — entity ↔ DTO mapping
- **Hangfire** — background jobs (SQL Server storage)
- **Serilog** — structured logging
- **ClosedXML + OpenXML SDK** — Excel read/write

### Database & Infrastructure
- **SQL Server 2019+** — collation `Vietnamese_CI_AS`
- **Redis** (StackExchange.Redis) — distributed cache
- **Docker + IIS** — deployment (on-premise)

### Frontend
- **React 18** + **TypeScript** (strict mode)
- **Vite** — build tool
- **Ant Design 5** — UI components
- **TanStack Query** — server state management
- **React Hook Form + Zod** — form handling
- **Recharts** — biểu đồ / dashboard
- **Zustand** — local state phức tạp

### Testing
- **xUnit** + **FluentAssertions** + **Moq**
- **Testcontainers** hoặc **SQLite in-memory** cho integration test

---

## 3. KIẾN TRÚC DATABASE (22 BẢNG — ĐÃ CHỐT)

### Dimension (danh mục tĩnh)
| Bảng | Mục đích |
|---|---|
| `Dim_Regions` | Tỉnh + Xã/Phường/Đặc khu (2 cấp, `LevelType IN (1,2)`) |
| `Dim_RegionMapping` | Mapping mã cũ ↔ mới sau sáp nhập 2025 |
| `Dim_VillageTypes` | 7 loại hình (LNTT, LNM, LNKP, NTT, NNM, NKP) |
| `Dim_IndustryGroups` | 7 nhóm ngành nghề |
| `Dim_OutputUnits` | Đơn vị sản lượng (tấn, kg, sản phẩm…) |
| `Dim_ExportCategories` | Mặt hàng TCMN xuất khẩu |
| `Dim_Classifications` | Làng nghề / Nghề |

### Master Data
| Bảng | Mục đích |
|---|---|
| `Dim_CraftVillages` | Làng nghề (mã 5 số: 2 số tỉnh + 3 số TT) |
| `Dim_Products` | Sản phẩm tiêu biểu + OCOP |
| `Dim_Artisans` | Nghệ nhân |

### Fact (dữ liệu động theo kỳ)
| Bảng | Đặc điểm |
|---|---|
| `Fact_Village_PeriodicMetrics` | 47 chỉ tiêu — **Partition by year + Columnstore index** |
| `Fact_Product_OCOP_History` | Lịch sử sao OCOP |
| `Fact_Village_Environment` | Hạ tầng & môi trường |
| `Fact_SubmissionWorkflow` | Workflow duyệt 2 bước |

### Aggregated (Data Warehouse)
| Bảng | Mục đích |
|---|---|
| `Agg_Village_TimeSeries` | Tổng hợp theo kỳ × vùng (2 cấp) |
| `Agg_By_IndustryGroup` | Tổng hợp theo nhóm ngành |
| `Agg_By_VillageType` | Tổng hợp theo loại hình |

### System & Import
| Bảng | Mục đích |
|---|---|
| `Sys_Users`, `Sys_Roles`, `Sys_Permissions`, `Sys_RolePermissions`, `Sys_UserRoles` | Phân quyền RBAC |
| `Sys_AuditLog` | Log mọi thao tác CUD |
| `Import_History`, `Staging_RawImport`, `Import_Error_Log` | Import pipeline |
| `Import_TemplateConfig`, `Sys_FormFieldConfig` | Cấu hình động |
| `Fact_Village_Draft` | Lưu nháp Form |

### Đặc điểm kỹ thuật QUAN TRỌNG
- **Partition by year** trên `Fact_Village_PeriodicMetrics` (2021–2030)
- **Columnstore Index** trên Fact table — tăng tốc OLAP 50-100x
- **Denormalize** `ProvinceId` + `CommuneId` vào Fact — bỏ JOIN khi query dashboard
- **Full-text search** trên `Dim_CraftVillages.Name` (LCID 1066 — tiếng Việt)
- **Computed columns PERSISTED**: `ExportValueVND`, `IsEffective`
- **Optimistic lock** bằng `ROWVERSION` trên `Dim_CraftVillages`

---

## 4. CẤU TRÚC SOLUTION

```
LangNghe.sln
├── src/
│   ├── LangNghe.Domain/          # Entities, Enums, Interfaces (không dependency)
│   ├── LangNghe.Infrastructure/  # EF DbContext, Repositories, Migrations
│   ├── LangNghe.Application/     # CQRS (Commands/Queries/Handlers), DTOs, Validators, Profiles
│   ├── LangNghe.Import/          # Excel parsers, ETL pipeline
│   ├── LangNghe.API/             # Controllers, Middleware, Program.cs, appsettings
│   └── LangNghe.Jobs/            # Hangfire background jobs
├── tests/
│   ├── LangNghe.UnitTests/       # Test business logic
│   └── LangNghe.IntegrationTests/ # Test API + DB
├── frontend/                      # React app
│   ├── src/
│   │   ├── components/           # Reusable components
│   │   ├── features/             # Feature-based folders
│   │   ├── hooks/                # Custom hooks
│   │   ├── services/             # API clients
│   │   ├── types/                # TypeScript types
│   │   └── utils/                # Helpers
├── docs/
│   ├── MASTER_PROMPT.md
│   ├── schema.sql
│   └── api-spec.yaml
├── AGENTS.md                     # File này
└── CLAUDE.md                     # Symlink hoặc copy của AGENTS.md
```

---

## 5. COMMANDS THƯỜNG DÙNG

```bash
# Build toàn solution
dotnet build

# Chạy unit test
dotnet test

# Chạy API (dev)
cd src/LangNghe.API && dotnet run

# Thêm migration
dotnet ef migrations add <MigrationName> \
  -p src/LangNghe.Infrastructure \
  -s src/LangNghe.API

# Update database
dotnet ef database update \
  -p src/LangNghe.Infrastructure \
  -s src/LangNghe.API

# Frontend
cd frontend && npm install && npm run dev
cd frontend && npm run build

# Docker
docker-compose up -d
```

---

## 6. CONVENTION BẮT BUỘC

### 6.1. Naming Conventions

| Loại | Quy tắc | Ví dụ |
|---|---|---|
| Entity | PascalCase, singular | `CraftVillage` (không `CraftVillages`) |
| DTO | `XxxDto` | `CreateVillageDto`, `VillageDetailDto` |
| Command | `<Động từ><Danh từ>Command` | `CreateVillageCommand` |
| Query | `Get<Danh từ>Query` | `GetVillagesQuery` |
| Handler | `<Command/Query>Handler` | `CreateVillageCommandHandler` |
| Validator | `<Command/Query>Validator` | `CreateVillageCommandValidator` |
| Controller | `<Danh từ số nhiều>Controller` | `VillagesController` |
| Service | `<Chức năng>Service` | `ExcelImportService` |
| Interface | `I<Service>` | `IExcelImportService` |
| Private field | `_camelCase` | `_unitOfWork` |
| Constant | `UPPER_SNAKE_CASE` | `MAX_PAGE_SIZE` |
| Async method | hậu tố `Async` | `GetVillagesAsync()` |

### 6.2. Ngôn ngữ

- **Code + identifiers (biến/hàm/class):** Tiếng Anh
- **Comment + XML doc:** Tiếng Việt có dấu
- **Message lỗi trả user:** Tiếng Việt có dấu

```csharp
// ✅ ĐÚNG
/// <summary>
/// Tạo mới làng nghề với kiểm tra quyền sở hữu theo xã.
/// </summary>
public async Task<int> CreateVillageAsync(CreateVillageDto dto, CancellationToken ct)
{
    // Kiểm tra user có quyền tạo LN cho xã này không
    await ValidateOwnershipAsync(dto.VillageCode, ct);
    // ...
}

// ❌ SAI
public async Task<int> CreateVillageAsync(CreateVillageDto dto)
{
    // check ownership
}
```

### 6.3. Response Format chuẩn (MỌI API)

```json
{
  "success": true,
  "data": { },
  "message": "Thao tác thành công",
  "errors": [],
  "timestamp": "2026-10-07T10:30:00Z"
}
```

### 6.4. Error Handling

| Exception | HTTP | Khi nào |
|---|---|---|
| `BusinessException` | 400 | Vi phạm nghiệp vụ |
| `NotFoundException` | 404 | Không tìm thấy entity |
| `UnauthorizedException` | 401 | Chưa đăng nhập |
| `ForbiddenException` | 403 | Không có quyền |
| `ValidationException` | 422 | Dữ liệu không hợp lệ |
| `Exception` (khác) | 500 | Lỗi hệ thống — log lại |

Global exception middleware bắt tất cả, log, trả response chuẩn.

---

## 7. PATTERN BẮT BUỘC

### 7.1. CQRS Pattern (MediatR)

```csharp
// Command
public record CreateVillageCommand(CreateVillageDto Dto) : IRequest<int>;

// Handler
public class CreateVillageCommandHandler : IRequestHandler<CreateVillageCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public async Task<int> Handle(CreateVillageCommand request, CancellationToken ct)
    {
        // 1. Validate ownership
        await ValidateOwnershipAsync(request.Dto, ct);
        
        // 2. Map + Insert
        var entity = request.Dto.Adapt<CraftVillage>();
        _unitOfWork.Repository<CraftVillage>().Add(entity);
        
        // 3. Save
        await _unitOfWork.SaveChangesAsync(ct);
        return entity.Id;
    }
}

// Controller mỏng
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateVillageDto dto)
{
    var id = await _mediator.Send(new CreateVillageCommand(dto));
    return Ok(ApiResponse.Ok(id, "Tạo làng nghề thành công"));
}
```

### 7.2. EF Core Tracking Rules

```csharp
// ✅ Read-only → AsNoTracking
var villages = await _context.CraftVillages
    .AsNoTracking()
    .Where(v => v.Status == 1)
    .Select(v => new VillageListDto { ... })  // Projection
    .ToListAsync(ct);

// ✅ Mutation → AsTracking rõ ràng (nếu Global NoTracking ON)
var village = await _context.CraftVillages
    .AsTracking()
    .FirstOrDefaultAsync(v => v.Id == id, ct);
village.Name = dto.Name;
await _context.SaveChangesAsync(ct);

// ✅ Bulk update → ExecuteUpdate (EF 7+)
await _context.Fact_Village_PeriodicMetrics
    .Where(f => f.ReportPeriod == "2026" && f.ProvinceId == 1)
    .ExecuteUpdateAsync(s => s.SetProperty(f => f.GrowthRate, 5.5m), ct);
```

### 7.3. Pagination chuẩn

```csharp
public record PagedResult<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalCount
)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

- **Default:** `page=1`, `pageSize=20`
- **Max:** `pageSize=200` — reject nếu vượt

### 7.4. Validation (FluentValidation)

```csharp
public class CreateVillageValidator : AbstractValidator<CreateVillageDto>
{
    public CreateVillageValidator()
    {
        RuleFor(x => x.VillageCode)
            .NotEmpty().WithMessage("Mã làng nghề bắt buộc")
            .Matches(@"^\d{5}$").WithMessage("Mã làng nghề phải có 5 chữ số");

        RuleFor(x => x.Name)
            .NotEmpty().MaximumLength(500);

        RuleFor(x => x)
            .Must(x => x.ExportRate + x.ImportMaterialRate <= 100)
            .WithMessage("Tổng tỷ lệ XK + NK không được vượt 100%");
    }
}
```

---

## 8. NGHIỆP VỤ THEN CHỐT

### 8.1. Phân quyền 3 cấp

| Cấp | DataScope | Phạm vi dữ liệu |
|---|---|---|
| TW (Bộ NN&PTNT) | 1 | Toàn quốc |
| Tỉnh (Sở NN&PTNT) | 2 | Trong tỉnh mình |
| Xã/Phường/Đặc khu | 3 | Trong xã mình |

**Cài đặt:** EF Core **Global Query Filter** trong `OnModelCreating`:

```csharp
modelBuilder.Entity<CraftVillage>()
    .HasQueryFilter(v => 
        _currentUser.DataScope == 1
     || (_currentUser.DataScope == 2 && v.Region.ParentId == _currentUser.RegionId)
     || (_currentUser.DataScope == 3 && v.RegionId == _currentUser.RegionId));
```

### 8.2. Ownership Rule (BẮT BUỘC kiểm tra)

**User cấp Xã chỉ được nhập/sửa làng nghề thuộc xã mình.**

```csharp
private async Task ValidateOwnershipAsync(CreateVillageDto dto, CancellationToken ct)
{
    if (_currentUser.DataScope != 3) return;  // Chỉ áp dụng cho cấp Xã

    var userRegion = await _regionRepo.GetByIdAsync(_currentUser.RegionId, ct);
    var provinceCodeInFile = dto.VillageCode.Substring(0, 2);
    
    // 2 số đầu mã LN phải khớp mã tỉnh của user
    if (provinceCodeInFile != userRegion.Parent.RegionCode)
        throw new BusinessException(
            $"Bạn chỉ được nhập dữ liệu của tỉnh {userRegion.Parent.Name}");
    
    // Nếu LN đã tồn tại, phải thuộc xã mình
    var existing = await _villageRepo.FindByCodeAsync(dto.VillageCode, ct);
    if (existing != null && existing.RegionId != userRegion.Id)
        throw new BusinessException("Làng nghề thuộc xã khác");
}
```

### 8.3. Import Excel — 3 Template × 4 Chế độ

**3 Template:**
- `V1_FULL` — 47 cột đầy đủ
- `V1_MINIMAL` — 8 cột required (VillageCode, VillageName, Province, Commune, RecognitionDate, Classification, VillageType, IndustryGroup)
- `MONTHLY` — 7 cột cập nhật số liệu

**4 Chế độ:**
- `MERGE` ⭐ khuyến nghị — chỉ update cột có trong file, giữ nguyên cột khác
- `REPLACE_FULL` — ghi đè toàn bộ
- `INSERT_ONLY` — chỉ thêm mới
- `UPDATE_ONLY` — chỉ update

**Rule import:**
- File **KHÔNG bắt buộc** đủ 47 cột — chỉ cần 8 cột required
- Lỗi > 10% → rollback toàn bộ
- Dùng `Staging_RawImport` để preview + rollback

### 8.4. Workflow phê duyệt (2 bước)

```
[Xã tạo nháp: 0] 
    ↓ Submit
[Chờ Tỉnh duyệt: 1] 
    ↓ Tỉnh duyệt → [Tỉnh OK: 2] → TW công bố → [TW CB: 3]
    ↓ Tỉnh trả lại → [Trả lại: 9] → Xã sửa lại
```

**Bảng lưu:** `Fact_SubmissionWorkflow`.

### 8.5. Dashboard & Báo cáo

- **Đọc từ `Agg_*` tables** — KHÔNG query `Fact_*` trực tiếp
- **Cache 3 tầng:** Memory (10s) → Redis (5p) → DB
- **Cache invalidation lan tỏa:** Xã → Tỉnh → TW
- **Hangfire job** rebuild `Agg_*` lúc 2h sáng

---

## 9. ANTI-PATTERNS — KHÔNG ĐƯỢC DÙNG

### SQL
- ❌ `SELECT *` trên bảng wide
- ❌ `WHERE YEAR(date) = 2026` (non-SARGable) → dùng range
- ❌ Correlated subquery → dùng JOIN + GROUP BY
- ❌ Cursor / WHILE loop trong SQL

### C# / EF Core
- ❌ `.Result` / `.Wait()` / `.GetAwaiter().GetResult()` — luôn dùng `async/await`
- ❌ Tracking entity cho read-only
- ❌ Load toàn bộ entity rồi `.Count()` — dùng `.CountAsync()`
- ❌ Load all vào memory rồi filter C# — dùng `.Where()` trên IQueryable
- ❌ N+1 query — dùng `.Include()` hoặc projection

### General
- ❌ Hardcode connection string, API key, secret
- ❌ `// TODO: ...` hoặc `...` trong code trả về
- ❌ Code không compile
- ❌ Trả code chia nhỏ kiểu "phần 1/3, 2/3" — phải hoàn chỉnh

---

## 10. CHECKLIST TRƯỚC KHI TRẢ CODE

### Backend
- [ ] Đã đọc `docs/schema.sql` để biết cột chính xác
- [ ] Tuân thủ CQRS (Command/Query + Handler riêng)
- [ ] DTO tách biệt cho từng use case
- [ ] FluentValidation cho input
- [ ] AutoMapper Profile cho mapping
- [ ] `async/await` + `CancellationToken` mọi nơi
- [ ] `AsNoTracking()` cho read-only
- [ ] Response format chuẩn `{ success, data, message, errors, timestamp }`
- [ ] Pagination cho list API
- [ ] Ownership check (nếu là mutation từ cấp Xã)

### Database Query
- [ ] SARGable predicate
- [ ] Covering index nếu query phức tạp
- [ ] Đọc `Agg_*` cho dashboard
- [ ] Không `SELECT *`

### Frontend
- [ ] TypeScript strict
- [ ] TanStack Query cho server state
- [ ] React Hook Form + Zod cho form
- [ ] Ant Design 5 components
- [ ] Không hardcode API URL — dùng env
- [ ] Error boundary + loading state

### Test
- [ ] Unit test cho business logic phức tạp
- [ ] Dùng `[Theory]` + `[InlineData]` cho data-driven
- [ ] Coverage ≥ 80% business logic

---

## 11. KHI NÀO DỪNG VÀ HỎI LẠI (KHÔNG TỰ ĐOÁN)

Dừng lại và hỏi user khi:

1. **Yêu cầu mâu thuẫn** với schema đã chốt
2. **Cần thêm bảng/cột** ngoài schema — đề xuất + chờ OK
3. **Cần package mới** ngoài tech stack — đề xuất + chờ OK
4. **Logic nghiệp vụ chưa rõ** — hỏi, không tự suy diễn
5. **Có nhiều cách hiểu** yêu cầu — hỏi để làm rõ
6. **Ảnh hưởng đến dữ liệu production** — luôn xác nhận trước

---

## 12. TÀI LIỆU THAM KHẢO

| File | Mục đích |
|---|---|
| `docs/schema.sql` | Database schema đầy đủ (chạy 1 lần) |
| `docs/MASTER_PROMPT.md` | Context chi tiết cho AI chat |
| `docs/api-spec.yaml` | OpenAPI spec các endpoint |
| `docs/business-rules.md` | Chi tiết nghiệp vụ |

---

## 13. VÍ DỤ MẪU — ĐỌC ĐỂ HIỂU STYLE

### Ví dụ 1: Endpoint lấy danh sách làng nghề

```csharp
// ============ QUERY ============
public record GetVillagesQuery(VillageFilterDto Filter) : IRequest<PagedResult<VillageListDto>>;

// ============ HANDLER ============
public class GetVillagesQueryHandler 
    : IRequestHandler<GetVillagesQuery, PagedResult<VillageListDto>>
{
    private readonly LangNgheDbContext _context;

    public async Task<PagedResult<VillageListDto>> Handle(
        GetVillagesQuery request, CancellationToken ct)
    {
        var query = _context.CraftVillages
            .AsNoTracking()
            .Where(v => v.Status == 1);

        // Filter động
        if (!string.IsNullOrEmpty(request.Filter.Keyword))
            query = query.Where(v => v.Name.Contains(request.Filter.Keyword));

        if (request.Filter.ProvinceId.HasValue)
            query = query.Where(v => v.Region.ParentId == request.Filter.ProvinceId.Value);

        // Đếm trước khi phân trang
        var total = await query.CountAsync(ct);

        // Projection + Pagination
        var items = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((request.Filter.Page - 1) * request.Filter.PageSize)
            .Take(request.Filter.PageSize)
            .Select(v => new VillageListDto
            {
                Id = v.Id,
                VillageCode = v.VillageCode,
                Name = v.Name,
                ProvinceName = v.Region.Parent.Name,
                CommuneName = v.Region.Name,
                VillageType = v.VillageType.Name
            })
            .ToListAsync(ct);

        return new PagedResult<VillageListDto>(
            items, request.Filter.Page, request.Filter.PageSize, total);
    }
}

// ============ CONTROLLER ============
[ApiController]
[Route("api/v1/villages")]
[Authorize]
public class VillagesController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] VillageFilterDto filter, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVillagesQuery(filter), ct);
        return Ok(ApiResponse.Ok(result));
    }
}
```

### Ví dụ 2: Component Form React

```tsx
// features/villages/components/VillageForm.tsx
import { Form, Tabs, Button, message } from 'antd';
import { useVillageForm } from '../hooks/useVillageForm';
import { TabBasicInfo } from './tabs/TabBasicInfo';
import { TabProductTech } from './tabs/TabProductTech';
// ...

export function VillageForm({ mode, villageId }: VillageFormProps) {
  const { form, isSubmitting, onSubmit, draftStatus } = useVillageForm({
    mode,
    villageId,
  });

  return (
    <Form form={form} layout="vertical" onFinish={onSubmit}>
      <Tabs
        items={[
          { key: 'basic', label: 'Thông tin cơ bản', children: <TabBasicInfo /> },
          { key: 'product', label: 'Sản phẩm & Công nghệ', children: <TabProductTech /> },
          // ...
        ]}
      />
      
      <div className="form-actions">
        {draftStatus === 'saving' && <span>Đang lưu nháp...</span>}
        {draftStatus === 'saved' && <span>✓ Đã lưu nháp</span>}
        
        <Button type="primary" htmlType="submit" loading={isSubmitting}>
          Gửi Tỉnh duyệt
        </Button>
      </div>
    </Form>
  );
}
```

---

**END OF AGENTS.md**
