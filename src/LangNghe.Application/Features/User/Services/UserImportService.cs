using System.Globalization;
using System.Net.Mail;
using ClosedXML.Excel;
using LangNghe.Application.Features.Auth.Interfaces;
using LangNghe.Application.Features.Import.Constants;
using LangNghe.Application.Features.User.Dtos;
using LangNghe.Application.Features.User.Interfaces;
using LangNghe.Domain.Entities;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.User.Services;

/// <summary>Đọc, xác thực và import danh sách người dùng từ Excel.</summary>
public class UserImportService : IUserImportService
{
    private static readonly string[] Headers = ["Username", "Password", "FullName", "Email", "Phone", "DataScope", "RegionCode", "RoleCodes"];
    private readonly ILangNgheDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUser;

    public UserImportService(ILangNgheDbContext context, IPasswordHasher passwordHasher, ICurrentUserService currentUser)
        => (_context, _passwordHasher, _currentUser) = (context, passwordHasher, currentUser);

    public async Task<UserImportPreviewDto> PreviewAsync(Stream stream, string importMode, CancellationToken cancellationToken)
    {
        ValidateMode(importMode);
        var rows = ReadRows(stream);
        var validation = await ValidateRowsAsync(rows, importMode, cancellationToken);
        return new UserImportPreviewDto(
            validation.Count,
            validation.Count(x => x.IsValid),
            validation.Count(x => !x.IsValid),
            validation.Select(x => x.Preview).ToList());
    }

    public async Task<UserImportResultDto> ImportAsync(Stream stream, string fileName, long fileSizeBytes, string importMode, bool skipErrors, CancellationToken cancellationToken)
    {
        ValidateMode(importMode);
        var parsedRows = ReadRows(stream);
        var validation = await ValidateRowsAsync(parsedRows, importMode, cancellationToken);
        var errors = validation.Where(x => !x.IsValid)
            .Select(x => new UserImportErrorDto(x.Row.RowNumber, x.Row.Username, string.Join("; ", x.Errors)))
            .ToList();

        if (validation.Count == 0) throw new BusinessException("File không có dữ liệu người dùng");
        if (errors.Count > validation.Count * 0.1 && !skipErrors)
            throw new BusinessException("Vượt ngưỡng lỗi 10%");

        var validRows = validation.Where(x => x.IsValid).ToList();
        var usernames = validRows.Select(x => x.Row.Username.ToLowerInvariant()).Distinct().ToList();
        var users = await _context.Users.IgnoreQueryFilters().AsTracking()
            .Include(x => x.UserRoles)
            .Where(x => usernames.Contains(x.Username.ToLower()))
            .ToListAsync(cancellationToken);
        var usersByName = users.ToDictionary(x => x.Username, StringComparer.OrdinalIgnoreCase);
        var now = DateTime.UtcNow;
        var batchId = Guid.NewGuid();
        var history = new ImportHistory
        {
            Id = batchId,
            FileName = fileName,
            FileSizeBytes = fileSizeBytes,
            TemplateCode = "USER_V1",
            ImportMode = importMode,
            UploadedBy = _currentUser.UserId ?? 0,
            UploadedByRegionId = _currentUser.RegionId,
            TotalRows = validation.Count,
            ErrorRows = errors.Count,
            SuccessRows = validRows.Count,
            Status = ImportStatus.Completed,
            StartedAt = now,
            CompletedAt = now,
            ErrorSummary = errors.Count == 0 ? null : string.Join(" | ", errors.Select(x => $"Dòng {x.RowNumber}: {x.Message}"))
        };
        _context.ImportHistories.Add(history);

        foreach (var error in errors)
        {
            _context.ImportErrorLogs.Add(new ImportErrorLog
            {
                ImportBatchId = batchId,
                RowNumber = error.RowNumber,
                ErrorCode = "USER_IMPORT_VALIDATION",
                ErrorMessage = error.Message,
                RawValue = error.Username,
                Severity = 1,
                CreatedAt = now
            });
        }

        var importedRows = 0;
        var updatedRows = 0;
        foreach (var item in validRows)
        {
            var row = item.Row;
            var normalizedUsername = row.Username.Trim();
            var isNew = !usersByName.TryGetValue(normalizedUsername, out var user);
            if (isNew)
            {
                user = new Domain.Entities.User
                {
                    Username = normalizedUsername,
                    PasswordHash = _passwordHasher.Hash(row.Password!),
                    IsActive = true,
                    MustChangePassword = true,
                    CreatedAt = now,
                    CreatedBy = _currentUser.UserId
                };
                _context.Users.Add(user);
                usersByName[normalizedUsername] = user;
                importedRows++;
            }
            else
            {
                updatedRows++;
                if (!string.IsNullOrWhiteSpace(row.Password)) user!.PasswordHash = _passwordHasher.Hash(row.Password);
            }

            user!.FullName = NullIfEmpty(row.FullName);
            user.Email = NullIfEmpty(row.Email);
            user.Phone = NullIfEmpty(row.Phone);
            user.DataScope = byte.Parse(row.DataScope, CultureInfo.InvariantCulture);
            user.RegionId = item.RegionId;
            if (!isNew)
            {
                user.UpdatedAt = now;
                user.UpdatedBy = _currentUser.UserId;
            }

            _context.UserRoles.RemoveRange(user.UserRoles);
            foreach (var roleId in item.RoleIds)
                _context.UserRoles.Add(new UserRole { User = user, RoleId = roleId, AssignedAt = now, AssignedBy = _currentUser.UserId });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return new UserImportResultDto(validation.Count, importedRows, updatedRows, errors.Count, errors.Count, errors);
    }

    public byte[] CreateTemplate()
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Users");
        for (var i = 0; i < Headers.Length; i++) sheet.Cell(1, i + 1).Value = Headers[i];
        sheet.Row(1).Style.Font.Bold = true;
        sheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        var examples = new object?[,] {
            { "xa_test", "ChangeMe123", "Cán bộ xã", "xa@example.com", "0912345678", "3", "00004", "XA_CB" },
            { "so_nn_test", "ChangeMe123", "Sở Nông nghiệp", "so@example.com", "0987654321", "2", "01", "SO_NN,VIEWER_TINH" },
            { "tw_test", "ChangeMe123", "Quản trị thử nghiệm", "tw@example.com", "0901234567", "1", "", "VIEWER_TW" }
        };
        for (var row = 0; row < examples.GetLength(0); row++)
            for (var column = 0; column < examples.GetLength(1); column++) sheet.Cell(row + 2, column + 1).Value = examples[row, column]?.ToString() ?? string.Empty;
        sheet.Columns().AdjustToContents();
        sheet.Column(2).Width = 18;
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private async Task<List<ValidatedRow>> ValidateRowsAsync(List<RawRow> rows, string importMode, CancellationToken cancellationToken)
    {
        var usernames = rows.Select(x => x.Username.Trim().ToLowerInvariant()).Where(x => x.Length > 0).Distinct().ToList();
        var users = await _context.Users.IgnoreQueryFilters().AsNoTracking().Where(x => usernames.Contains(x.Username.ToLower())).Select(x => x.Username).ToListAsync(cancellationToken);
        var existingUsers = users.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var regionRows = await _context.Regions.AsNoTracking().Where(x => x.IsActive).Select(x => new { x.Id, x.RegionCode }).ToListAsync(cancellationToken);
        var regions = regionRows.ToDictionary(x => x.RegionCode, x => x.Id, StringComparer.OrdinalIgnoreCase);
        var roleRows = await _context.Roles.AsNoTracking().Where(x => x.IsActive).Select(x => new { x.Id, x.Code }).ToListAsync(cancellationToken);
        var roles = roleRows.ToDictionary(x => x.Code, x => x.Id, StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<ValidatedRow>();

        foreach (var row in rows)
        {
            var errors = new List<string>();
            var username = row.Username.Trim();
            var exists = existingUsers.Contains(username);
            if (string.IsNullOrWhiteSpace(username) || username.Length is < 3 or > 100 || !System.Text.RegularExpressions.Regex.IsMatch(username, "^[a-zA-Z0-9_]+$")) errors.Add("Username bắt buộc, dài 3-100 ký tự và chỉ gồm chữ, số, dấu gạch dưới");
            if (!seen.Add(username)) errors.Add("Username bị trùng trong file");
            if (importMode == ImportMode.INSERT_ONLY && exists) errors.Add("Username đã tồn tại");
            if (importMode == ImportMode.UPDATE_ONLY && !exists) errors.Add("Username chưa tồn tại");
            var needsPassword = importMode == ImportMode.INSERT_ONLY || (importMode == ImportMode.UPSERT && !exists);
            if (needsPassword && string.IsNullOrWhiteSpace(row.Password)) errors.Add("Password là bắt buộc khi tạo user");
            if (!string.IsNullOrWhiteSpace(row.Password) && !IsValidPassword(row.Password)) errors.Add("Password tối thiểu 8 ký tự, gồm chữ và số");
            if (!byte.TryParse(row.DataScope, NumberStyles.Integer, CultureInfo.InvariantCulture, out var dataScope) || dataScope is < 1 or > 3) errors.Add("DataScope chỉ nhận 1, 2 hoặc 3");
            var regionCode = NullIfEmpty(row.RegionCode);
            int? regionId = null;
            if (dataScope is 2 or 3)
            {
                if (regionCode == null) errors.Add("RegionCode bắt buộc với DataScope 2 hoặc 3");
                else if (!regions.TryGetValue(regionCode, out var foundRegion)) errors.Add("RegionCode không tồn tại");
                else regionId = foundRegion;
            }
            else if (regionCode != null)
            {
                if (regions.TryGetValue(regionCode, out var foundRegion)) regionId = foundRegion;
                else errors.Add("RegionCode không tồn tại");
            }
            if (!string.IsNullOrWhiteSpace(row.Email) && !IsValidEmail(row.Email)) errors.Add("Email không hợp lệ");
            if (!string.IsNullOrWhiteSpace(row.Phone) && !System.Text.RegularExpressions.Regex.IsMatch(row.Phone.Trim(), "^(0|\\+84)[3|5|7|8|9][0-9]{8}$")) errors.Add("Số điện thoại không hợp lệ");
            var roleCodes = SplitCodes(row.RoleCodes);
            var roleIds = new List<int>();
            foreach (var roleCode in roleCodes)
            {
                if (!roles.TryGetValue(roleCode, out var roleId)) errors.Add($"RoleCode không tồn tại: {roleCode}");
                else roleIds.Add(roleId);
            }
            var dataScopeName = dataScope switch { 1 => "Toàn quốc", 2 => "Tỉnh", 3 => "Xã", _ => row.DataScope };
            result.Add(new ValidatedRow(row, regionId, roleIds, errors, new UserImportRowDto(row.RowNumber, username, NullIfEmpty(row.FullName), dataScopeName, regionCode, roleCodes, errors.Count == 0, errors)));
        }
        return result;
    }

    private static List<RawRow> ReadRows(Stream stream)
    {
        if (stream.CanSeek) stream.Position = 0;
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheets.FirstOrDefault() ?? throw new BusinessException("File Excel không có worksheet");
        var header = sheet.Row(1);
        for (var i = 0; i < Headers.Length; i++)
            if (!string.Equals(header.Cell(i + 1).GetString().Trim(), Headers[i], StringComparison.OrdinalIgnoreCase)) throw new BusinessException($"Header không hợp lệ tại cột {i + 1}: cần {Headers[i]}");
        var rows = new List<RawRow>();
        var range = sheet.RangeUsed();
        if (range == null) return rows;
        foreach (var row in range.RowsUsed().Skip(1))
        {
            var values = Enumerable.Range(1, Headers.Length).Select(i => row.Cell(i).GetString().Trim()).ToArray();
            if (values.All(string.IsNullOrWhiteSpace)) continue;
            rows.Add(new RawRow(row.RowNumber(), values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7]));
        }
        return rows;
    }

    private static void ValidateMode(string importMode)
    {
        if (importMode != ImportMode.INSERT_ONLY && importMode != ImportMode.UPDATE_ONLY && importMode != ImportMode.UPSERT) throw new BusinessException("Import mode không hợp lệ");
    }

    private static bool IsValidPassword(string password) => password.Length >= 8 && password.Any(char.IsLetter) && password.Any(char.IsDigit);
    private static bool IsValidEmail(string email) { try { _ = new MailAddress(email.Trim()); return true; } catch (FormatException) { return false; } }
    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static List<string> SplitCodes(string value) => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

    private sealed record RawRow(int RowNumber, string Username, string Password, string FullName, string Email, string Phone, string DataScope, string RegionCode, string RoleCodes);
    private sealed record ValidatedRow(RawRow Row, int? RegionId, List<int> RoleIds, List<string> Errors, UserImportRowDto Preview) { public bool IsValid => Errors.Count == 0; }
}
