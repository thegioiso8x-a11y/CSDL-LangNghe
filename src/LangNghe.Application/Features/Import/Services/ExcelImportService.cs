using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using LangNghe.Application.Features.Import.Dtos;
using LangNghe.Application.Features.Import.Interfaces;
using LangNghe.Domain.Entities;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Import.Services;

/// <summary>
/// Service xử lý import excel
/// </summary>
public class ExcelImportService : IExcelImportService
{
    private readonly IExcelTemplateService _templateService;
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IVillageOwnershipService _villageOwnershipService;

    public ExcelImportService(
        IExcelTemplateService templateService,
        ILangNgheDbContext context,
        ICurrentUserService currentUserService,
        IVillageOwnershipService villageOwnershipService)
    {
        _templateService = templateService;
        _context = context;
        _currentUserService = currentUserService;
        _villageOwnershipService = villageOwnershipService;
    }

    public async Task<List<Dictionary<string, object?>>> ParseAsync(Stream stream, string templateCode, CancellationToken ct)
    {
        var result = new List<Dictionary<string, object?>>();
        var configs = await _templateService.GetTemplateConfigsAsync(templateCode, ct);

        if (!configs.Any())
            return result;

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using var workbook = new XLWorkbook(stream);

        // Tìm Sheet có chứa nhiều cột khớp nhất với Template cấu hình
        var expectedHeaders = configs.Select(c => c.ExcelColumnHeader.Trim().ToLowerInvariant()).ToHashSet();
        IXLWorksheet? worksheet = null;
        int maxMatchCount = 0;

        foreach (var ws in workbook.Worksheets)
        {
            var wsHeaderRow = ws.Row(1);
            var lastCell = wsHeaderRow.LastCellUsed();
            if (lastCell == null) continue;

            int matchCount = 0;
            for (int i = 1; i <= lastCell.Address.ColumnNumber; i++)
            {
                var headerValue = wsHeaderRow.Cell(i).GetString()?.Trim().ToLowerInvariant();
                if (!string.IsNullOrEmpty(headerValue) && expectedHeaders.Contains(headerValue))
                {
                    matchCount++;
                }
            }

            if (matchCount > maxMatchCount)
            {
                maxMatchCount = matchCount;
                worksheet = ws;
            }
        }

        // Fallback nếu không tìm thấy Sheet nào khớp
        worksheet ??= workbook.Worksheets.FirstOrDefault();

        if (worksheet == null)
            return result;

        var headerRow = worksheet.Row(1);
        var lastCellUsed = headerRow.LastCellUsed();
        if (lastCellUsed == null)
            return result;

        int lastColNumber = lastCellUsed.Address.ColumnNumber;
        
        var colMap = new Dictionary<int, (string FieldName, string DataType)>();

        for (int i = 1; i <= lastColNumber; i++)
        {
            var headerValue = headerRow.Cell(i).GetString()?.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(headerValue)) continue;

            var matchConfig = configs.FirstOrDefault(c => c.ExcelColumnHeader.Trim().ToLowerInvariant() == headerValue);
            if (matchConfig != null)
            {
                colMap[i] = (matchConfig.FieldName, matchConfig.DataType.ToLowerInvariant());
            }
        }

        var rangeUsed = worksheet.RangeUsed();
        if (rangeUsed == null)
            return result;
            
        var rowsUsed = rangeUsed.RowsUsed().Skip(1);

        foreach (var row in rowsUsed)
        {
            var rowData = new Dictionary<string, object?>();

            foreach (var config in configs)
            {
                rowData[config.FieldName] = null;
            }

            bool isEmptyRow = true;

            foreach (var kvp in colMap)
            {
                int colIndex = kvp.Key;
                var fieldName = kvp.Value.FieldName;
                var dataType = kvp.Value.DataType;

                var cell = row.Cell(colIndex);
                var cellValueStr = cell.GetString()?.Trim();

                if (!string.IsNullOrEmpty(cellValueStr))
                {
                    isEmptyRow = false;
                    // Các cột khóa ngoại có thể chứa Mã (Code) dạng chuỗi có số 0 ở đầu (VD: "01576").
                    // Giữ nguyên dạng string để ImportAsync resolve Code -> Id chính xác.
                    if (IsForeignKeyField(fieldName))
                    {
                        rowData[fieldName] = cellValueStr;
                    }
                    else
                    {
                        rowData[fieldName] = ParseValue(cellValueStr, dataType);
                    }
                }
            }

            if (!isEmptyRow)
            {
                result.Add(rowData);
            }
        }

        return result;
    }

    public async Task<ImportResultDto> ImportAsync(
        Stream stream,
        string fileName,
        long fileSizeBytes,
        string templateCode,
        string importMode,
        CancellationToken ct)
    {
        var batchId = Guid.NewGuid();
        var userId = _currentUserService.UserId ?? 0;
        
        var history = new ImportHistory
        {
            Id = batchId,
            FileName = fileName,
            FileSizeBytes = fileSizeBytes,
            TemplateCode = templateCode,
            ImportMode = importMode,
            UploadedBy = userId,
            UploadedByRegionId = _currentUserService.RegionId,
            Status = 0, 
            StartedAt = DateTime.UtcNow
        };
        
        _context.ImportHistories.Add(history);
        
        var parsedRows = await ParseAsync(stream, templateCode, ct);
        int totalRows = parsedRows.Count;
        history.TotalRows = totalRows;
        
        if (totalRows == 0)
        {
            history.Status = 1; 
            history.CompletedAt = DateTime.UtcNow;
            return new ImportResultDto(batchId, 0, 0, 0, 0, 1, "File không có dữ liệu hợp lệ");
        }

        int rowNumber = 2; 
        var stags = new List<RawImportStaging>();
        foreach (var rowData in parsedRows)
        {
            stags.Add(new RawImportStaging
            {
                ImportBatchId = batchId,
                RowNumber = rowNumber++,
                RawData = JsonSerializer.Serialize(rowData),
                ValidationStatus = 0, 
                CreatedAt = DateTime.UtcNow
            });
        }
        _context.RawImportStagings.AddRange(stags);
        
        int errorRows = 0;
        int successRows = 0;
        int warningRows = 0;
        
        var configs = await _templateService.GetTemplateConfigsAsync(templateCode, ct);
        var reqFields = configs.Where(x => x.IsRequired).ToList();
        
        var villageCodes = parsedRows.Select(r => r.GetValueOrDefault("VillageCode")?.ToString())
                                     .Where(v => !string.IsNullOrWhiteSpace(v))
                                     .ToList();
                                     
        var existingVillages = await _context.CraftVillages
            .AsNoTracking()
            .Where(v => villageCodes.Contains(v.VillageCode))
            .ToDictionaryAsync(v => v.VillageCode, ct);
            
        // Mã tỉnh của user (dùng cho DataScope = 2). Với DataScope = 3 sẽ kiểm tra RegionId chính xác.
        string? userProvinceCode = null;
        if (_currentUserService.DataScope == 2 && _currentUserService.RegionId.HasValue)
        {
            var userRegion = await _context.Regions.AsNoTracking().FirstOrDefaultAsync(r => r.Id == _currentUserService.RegionId.Value, ct);
            if (userRegion != null)
            {
                userProvinceCode = userRegion.RegionCode;
            }
        }

        var errorsToSave = new List<ImportErrorLog>();
        
        // ===== Ánh xạ Mã (Code) -> ID cho các trường khóa ngoại =====
        // Người dùng có thể nhập Mã xã/Phân loại/Loại hình/Nhóm ngành (chuỗi) thay vì ID (số nguyên).
        var regionCodeToId = await _context.Regions
            .AsNoTracking()
            .Where(r => r.LevelType == 2 && r.IsActive)
            .ToDictionaryAsync(r => r.RegionCode, r => r.Id, ct);
        var regionIds = regionCodeToId.Values.ToHashSet();

        var classificationCodeToId = await _context.Classifications
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Code, c => c.Id, ct);
        var classificationIds = classificationCodeToId.Values.ToHashSet();

        var villageTypeCodeToId = await _context.VillageTypes
            .AsNoTracking()
            .ToDictionaryAsync(v => v.Code, v => v.Id, ct);
        var villageTypeIds = villageTypeCodeToId.Values.ToHashSet();

        var industryGroupCodeToId = await _context.IndustryGroups
            .AsNoTracking()
            .ToDictionaryAsync(g => g.Code, g => g.Id, ct);
        var industryGroupIds = industryGroupCodeToId.Values.ToHashSet();

        // Hàm nội bộ: resolve giá trị FK (Code hoặc Id) -> Id (int). Trả về null nếu không hợp lệ.
        int? ResolveForeignKey(
            Dictionary<string, object?> data,
            string fieldName,
            Dictionary<string, int> codeToId,
            HashSet<int> validIds)
        {
            if (!data.TryGetValue(fieldName, out var raw) || raw == null)
                return null;

            var text = raw.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return null;

            // 1) Khớp theo Mã (Code) — ưu tiên vì mã xã dạng "01576" có thể bị parse thành int 1576
            if (codeToId.TryGetValue(text, out var idByCode))
                return idByCode;

            // 2) Nếu là số và tồn tại trong DB -> coi là ID hợp lệ
            if (int.TryParse(text, out var idByNumber) && validIds.Contains(idByNumber))
                return idByNumber;

            return null;
        }

        // Danh sách 4 trường FK cần resolve: (FieldName, Tên cột hiển thị, codeToId, validIds)
        var fkFields = new (string FieldName, string ColumnName, Dictionary<string, int> CodeToId, HashSet<int> ValidIds)[]
        {
            ("RegionId", "Mã xã", regionCodeToId, regionIds),
            ("ClassificationId", "Phân loại", classificationCodeToId, classificationIds),
            ("VillageTypeId", "Loại hình", villageTypeCodeToId, villageTypeIds),
            ("IndustryGroupId", "Nhóm ngành", industryGroupCodeToId, industryGroupIds),
        };

        for(int i = 0; i < stags.Count; i++)
        {
            var rowData = parsedRows[i];
            var stag = stags[i];
            bool isRowValid = true;
            var rowErrors = new List<string>();
            
            foreach(var req in reqFields)
            {
                if (!rowData.ContainsKey(req.FieldName) || rowData[req.FieldName] == null || string.IsNullOrWhiteSpace(rowData[req.FieldName]?.ToString()))
                {
                    isRowValid = false;
                    errorsToSave.Add(new ImportErrorLog
                    {
                        ImportBatchId = batchId,
                        RowNumber = stag.RowNumber,
                        ColumnName = req.ExcelColumnHeader,
                        ErrorCode = "REQUIRED",
                        ErrorMessage = $"Thiếu dữ liệu bắt buộc: {req.ExcelColumnHeader}",
                        Severity = 1,
                        CreatedAt = DateTime.UtcNow
                    });
                    rowErrors.Add($"Thiếu dữ liệu bắt buộc: {req.ExcelColumnHeader}");
                }
            }
            
            // ===== Resolve 4 trường khóa ngoại: Mã (Code) -> ID =====
            foreach (var fk in fkFields)
            {
                if (!rowData.TryGetValue(fk.FieldName, out var rawFk) || rawFk == null)
                    continue;

                var rawText = rawFk.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(rawText))
                    continue;

                var resolvedId = ResolveForeignKey(rowData, fk.FieldName, fk.CodeToId, fk.ValidIds);
                if (resolvedId.HasValue)
                {
                    // Gán đè bằng int để UpdateVillageProperty hiểu đúng kiểu
                    rowData[fk.FieldName] = resolvedId.Value;
                }
                else
                {
                    isRowValid = false;
                    var msg = $"Mã {fk.ColumnName} không tồn tại trong hệ thống";
                    errorsToSave.Add(new ImportErrorLog
                    {
                        ImportBatchId = batchId,
                        RowNumber = stag.RowNumber,
                        ColumnName = fk.ColumnName,
                        ErrorCode = "INVALID_FK",
                        ErrorMessage = msg,
                        Severity = 1,
                        CreatedAt = DateTime.UtcNow
                    });
                    rowErrors.Add(msg);
                }
            }

            var vCodeObj = rowData.GetValueOrDefault("VillageCode");
            var vCode = vCodeObj?.ToString();
            
            if (!string.IsNullOrWhiteSpace(vCode))
            {
                stag.ParsedVillageCode = vCode;
                bool exists = existingVillages.ContainsKey(vCode);
                
                if (importMode == "INSERT_ONLY" && exists)
                {
                    isRowValid = false;
                    var msg = "Làng nghề đã tồn tại";
                    errorsToSave.Add(new ImportErrorLog { ImportBatchId = batchId, RowNumber = stag.RowNumber, ErrorMessage = msg, Severity = 1, CreatedAt = DateTime.UtcNow });
                    rowErrors.Add(msg);
                }
                else if (importMode == "UPDATE_ONLY" && !exists)
                {
                    isRowValid = false;
                    var msg = "Không tìm thấy làng nghề";
                    errorsToSave.Add(new ImportErrorLog { ImportBatchId = batchId, RowNumber = stag.RowNumber, ErrorMessage = msg, Severity = 1, CreatedAt = DateTime.UtcNow });
                    rowErrors.Add(msg);
                }
                
                // ===== Kiểm tra quyền theo địa bàn (Data Scope Isolation) =====
                if (_currentUserService.DataScope == 2)
                {
                    // Tỉnh: mã làng nghề phải bắt đầu bằng mã tỉnh của user
                    if (userProvinceCode != null && !vCode.StartsWith(userProvinceCode))
                    {
                        isRowValid = false;
                        var msg = "Làng nghề không thuộc thẩm quyền quản lý";
                        errorsToSave.Add(new ImportErrorLog { ImportBatchId = batchId, RowNumber = stag.RowNumber, ErrorMessage = msg, Severity = 1, CreatedAt = DateTime.UtcNow });
                        rowErrors.Add(msg);
                    }
                }
                else if (_currentUserService.DataScope == 3)
                {
                    // Xã: RegionId (đã resolve từ Code) phải trùng khớp chính xác với xã của user
                    var resolvedRegionId = rowData.TryGetValue("RegionId", out var regionRaw) && regionRaw is int rid ? rid : (int?)null;
                    if (!_currentUserService.RegionId.HasValue || resolvedRegionId != _currentUserService.RegionId.Value)
                    {
                        isRowValid = false;
                        var msg = "Làng nghề không thuộc xã/phường quản lý";
                        errorsToSave.Add(new ImportErrorLog { ImportBatchId = batchId, RowNumber = stag.RowNumber, ErrorMessage = msg, Severity = 1, CreatedAt = DateTime.UtcNow });
                        rowErrors.Add(msg);
                    }
                }
            }
            else
            {
                isRowValid = false;
            }
            
            if (!isRowValid)
            {
                stag.ValidationStatus = 2; // Invalid
                stag.ValidationErrors = JsonSerializer.Serialize(rowErrors);
                errorRows++;
            }
            else
            {
                stag.ValidationStatus = 1; // Valid
            }
        }
        
        if (errorsToSave.Any())
        {
            _context.ImportErrorLogs.AddRange(errorsToSave);
        }
        
        if (totalRows > 0 && (double)errorRows / totalRows > 0.10)
        {
            history.Status = 2; // Failed
            history.ErrorSummary = "Vượt ngưỡng lỗi 10%";
            history.ErrorRows = errorRows;
            history.SuccessRows = successRows;
            await _context.SaveChangesAsync(ct);
            return new ImportResultDto(batchId, totalRows, successRows, errorRows, warningRows, 2, "Import thất bại do vượt ngưỡng lỗi 10%");
        }
        
        var stagValid = stags.Where(s => s.ValidationStatus == 1).ToList();
        var validVillageCodes = stagValid.Select(s => s.ParsedVillageCode!).Distinct().ToList();
        
        var trackedVillages = await _context.CraftVillages
            .Where(v => validVillageCodes.Contains(v.VillageCode))
            .ToDictionaryAsync(v => v.VillageCode, ct);

        foreach (var stag in stagValid)
        {
            var rowData = parsedRows[stag.RowNumber - 2];
            var vCode = stag.ParsedVillageCode!;
            
            trackedVillages.TryGetValue(vCode, out var existing);
            
            if (importMode == "INSERT_ONLY" && existing != null) { continue; }
            if (importMode == "UPDATE_ONLY" && existing == null) { continue; }
            
            if (existing != null)
            {
                foreach (var kvp in rowData)
                {
                    if (importMode == "MERGE" && kvp.Value == null) continue;
                    UpdateVillageProperty(existing, kvp.Key, kvp.Value);
                }
                
                stag.PersistedEntityId = existing.Id;
            }
            else
            {
                var newVillage = new CraftVillage { VillageCode = vCode, Name = string.Empty };
                foreach (var kvp in rowData)
                {
                    if (kvp.Value != null)
                    {
                        UpdateVillageProperty(newVillage, kvp.Key, kvp.Value);
                    }
                }
                
                if (string.IsNullOrEmpty(newVillage.Name))
                    newVillage.Name = "N/A";

                _context.CraftVillages.Add(newVillage);
                trackedVillages[vCode] = newVillage; 
            }
            
            stag.ValidationStatus = 3; 
            stag.PersistedAt = DateTime.UtcNow;
            successRows++;
        }
        
        history.SuccessRows = successRows;
        history.ErrorRows = errorRows;
        history.WarningRows = warningRows;
        history.Status = 1; 
        history.CompletedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync(ct);
        
        foreach (var stag in stagValid)
        {
            if (stag.PersistedEntityId == null && stag.ParsedVillageCode != null)
            {
                if (trackedVillages.TryGetValue(stag.ParsedVillageCode, out var v))
                {
                    stag.PersistedEntityId = v.Id;
                }
            }
        }
        await _context.SaveChangesAsync(ct);
        
        return new ImportResultDto(batchId, totalRows, successRows, errorRows, warningRows, 1, "Import thành công");
    }

    public async Task<List<ImportPreviewRowDto>> PreviewAsync(Guid batchId, CancellationToken ct)
    {
        var stags = await _context.RawImportStagings
            .AsNoTracking()
            .Where(x => x.ImportBatchId == batchId)
            .OrderBy(x => x.RowNumber)
            .ToListAsync(ct);
            
        var result = new List<ImportPreviewRowDto>();
        foreach(var stag in stags)
        {
            var rowData = JsonSerializer.Deserialize<Dictionary<string, object?>>(stag.RawData);
            string? vCode = stag.ParsedVillageCode;
            
            string? vName = null;
            if (rowData != null && rowData.TryGetValue("Name", out var nameObj) && nameObj != null)
            {
                if (nameObj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.String)
                {
                    vName = jsonElement.GetString();
                }
                else
                {
                    vName = nameObj.ToString();
                }
            }
            
            List<string> errors = new List<string>();
            if (!string.IsNullOrEmpty(stag.ValidationErrors))
            {
                try
                {
                    errors = JsonSerializer.Deserialize<List<string>>(stag.ValidationErrors) ?? new List<string>();
                }
                catch { }
            }
            
            result.Add(new ImportPreviewRowDto(stag.RowNumber, vCode, vName, stag.ValidationStatus, errors));
        }
        return result;
    }

    private void UpdateVillageProperty(CraftVillage village, string propertyName, object? value)
    {
        if (propertyName == "VillageCode" && !string.IsNullOrEmpty(village.VillageCode))
            return; 

        var prop = typeof(CraftVillage).GetProperty(propertyName);
        if (prop != null && prop.CanWrite)
        {
            if (value == null)
            {
                if (!prop.PropertyType.IsValueType || Nullable.GetUnderlyingType(prop.PropertyType) != null)
                {
                    prop.SetValue(village, null);
                }
            }
            else
            {
                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                try
                {
                    object convertedValue = value;
                    if (value is JsonElement jsonElement)
                    {
                        if (targetType == typeof(string)) convertedValue = jsonElement.GetString()!;
                        else if (targetType == typeof(int)) convertedValue = jsonElement.GetInt32();
                        else if (targetType == typeof(long)) convertedValue = jsonElement.GetInt64();
                        else if (targetType == typeof(double)) convertedValue = jsonElement.GetDouble();
                        else if (targetType == typeof(decimal)) convertedValue = jsonElement.GetDecimal();
                        else if (targetType == typeof(bool)) convertedValue = jsonElement.GetBoolean();
                        else if (targetType == typeof(DateTime)) convertedValue = jsonElement.GetDateTime();
                    }
                    else
                    {
                        convertedValue = Convert.ChangeType(value, targetType);
                    }
                    prop.SetValue(village, convertedValue);
                }
                catch { }
            }
        }
    }

    private object? ParseValue(string value, string dataType)
    {
        try
        {
            return dataType switch
            {
                "int" => int.TryParse(value, out int intVal) ? intVal : null,
                "decimal" => decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decVal) ? decVal : null,
                "datetime" => DateTime.TryParse(value, out DateTime dtVal) ? dtVal : null,
                "bool" => IsTrue(value),
                "string" => value,
                _ => value
            };
        }
        catch
        {
            return null;
        }
    }

    private bool IsTrue(string value)
    {
        var lower = value.ToLowerInvariant();
        return lower == "x" || lower == "1" || lower == "true" || lower == "yes";
    }

    /// <summary>
    /// Các cột khóa ngoại có thể nhập Mã (Code) dạng chuỗi (VD: "01576") thay vì ID số nguyên.
    /// Cần giữ nguyên dạng string khi parse để resolve Code -> Id chính xác.
    /// </summary>
    private static bool IsForeignKeyField(string fieldName)
    {
        return fieldName is "RegionId" or "ClassificationId" or "VillageTypeId" or "IndustryGroupId";
    }
}