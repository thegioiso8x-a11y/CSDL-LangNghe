# CẤU TRÚC SOLUTION

```
LangNghe.sln
├── src/
│   ├── LangNghe.Domain/          # Entities, Enums, Interfaces, Exceptions, Constants
│   ├── LangNghe.Infrastructure/  # EF DbContext, Repositories, Services
│   ├── LangNghe.Application/     # CQRS, DTOs, Validators, Mappings, Behaviors
│   ├── LangNghe.API/             # Controllers, Middleware, Program.cs
│   ├── LangNghe.Import/          # ⏳ chưa tạo
│   └── LangNghe.Jobs/            # ⏳ chưa tạo
├── tests/                         # ⏳ chưa tạo
├── frontend/                      # ⏳ chưa tạo
└── docs/
    ├── AGENTS.md
    ├── handoff.md + handoff/
    ├── MASTER_PROMPT.md
    ├── README.md
    ├── 01_schema_v3.sql
    └── migrations/
```

## Dependency chain

```
Domain ← Application ← Infrastructure ← API
```

⚠️ **Lưu ý đặc biệt:** `Infrastructure` có `ProjectReference` đến `Application` — vì `IAuthService`, `IJwtTokenService`, `IPasswordHasher`, `ICaptchaService` đặt ở `Application/Features/Auth/Interfaces/`. Đây là **chủ ý**, không phải bug.

## DbContext pattern

- Interface `ILangNgheDbContext` ở `LangNghe.Domain.Interfaces`
- Class `LangNgheDbContext` ở `LangNghe.Infrastructure.Persistence` implements interface
- DI: `services.AddScoped<ILangNgheDbContext>(sp => sp.GetRequiredService<LangNgheDbContext>())`
- Application layer CHỈ inject `ILangNgheDbContext`

## Feature folder pattern

```
Features/{Module}/
├── Dtos/         # XxxDto, CreateXxxDto, UpdateXxxDto, XxxFilterDto
├── Queries/      # GetXxxListQuery, GetXxxByIdQuery (+ Handler)
├── Commands/     # CreateXxxCommand, UpdateXxxCommand, DeleteXxxCommand (+ Handler)
├── Validators/   # CreateXxxValidator, UpdateXxxValidator
└── Mappings/     # XxxProfile (AutoMapper)
```

Controller đặt ở `LangNghe.API/Controllers/`.
