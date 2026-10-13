# TECH STACK

## Backend — .NET 10

| Layer | Công nghệ | Version |
|---|---|---|
| Runtime | .NET | 10.0 (`net10.0`) |
| Language | C# | 14 |
| ORM | EF Core | 10.0.0 |
| CQRS | MediatR | 12.4.1 |
| Validation | FluentValidation.DI | 11.9.2 |
| Mapping | AutoMapper | 13.0.1 ⚠️ NU1903 |
| JWT Bearer | Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.12 |
| JWT gen | System.IdentityModel.Tokens.Jwt | 8.x |
| Password Hash | BCrypt.Net-Next | 4.0.3 |
| Swagger | Swashbuckle.AspNetCore | 6.6.2 |
| Logging | Serilog.AspNetCore | 8.0.3 |

## ⚠️ Lưu ý .NET 10
- `Microsoft.AspNetCore.Authentication.JwtBearer` **KHÔNG có** trong shared framework → phải cài NuGet.
- `System.Security.Cryptography.Xml 9.0.0` có NU1903.
- AutoMapper 13.0.1 có NU1903 — cần nâng cấp sau.

## Database & Infra
- SQL Server 2022, DB `CSDL_LangNghe`, collation `Vietnamese_CI_AS`
- Redis (chưa cấu hình)
- Docker + IIS (chưa deploy)

## Connection string (dev)
Server=***REDACTED_DB_HOST***,1433;Database=CSDL_LangNghe;User Id=sa;Password=***REDACTED_SQL_PWD***;TrustServerCertificate=True;MultipleActiveResultSets=True

## Frontend (chưa làm)
React 18 + TS (strict) + Vite + AntD 5 + TanStack Query + RHF/Zod + Recharts + Zustand
