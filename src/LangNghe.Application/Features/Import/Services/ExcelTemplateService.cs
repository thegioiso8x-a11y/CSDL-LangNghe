using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using LangNghe.Application.Common;
using LangNghe.Application.Features.Import.Interfaces;
using LangNghe.Domain.Entities;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Import.Services;

/// <summary>
/// Service xử lý template excel
/// </summary>
public class ExcelTemplateService : IExcelTemplateService
{
    private readonly ILangNgheDbContext _context;

    public ExcelTemplateService(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<List<ImportTemplateConfig>> GetTemplateConfigsAsync(string templateCode, CancellationToken ct)
    {
        return await _context.ImportTemplateConfigs
            .AsNoTracking()
            .Where(x => x.TemplateCode == templateCode && x.IsActive)
            .OrderBy(x => x.FieldOrder)
            .ToListAsync(ct);
    }

    public async Task<string> DetectTemplateAsync(Stream stream, CancellationToken ct)
    {
        var allConfigs = await _context.ImportTemplateConfigs
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync(ct);

        if (!allConfigs.Any())
            return string.Empty;

        var allExpectedHeaders = allConfigs.Select(x => x.ExcelColumnHeader.Trim().ToLowerInvariant()).Distinct().ToList();
        var actualHeaders = GetHeadersFromStream(stream, allExpectedHeaders);
        if (!actualHeaders.Any())
            return string.Empty;

        var groupedConfigs = allConfigs.GroupBy(x => x.TemplateCode);
        
        string bestMatchTemplate = string.Empty;
        int maxMatchCount = -1;

        foreach (var group in groupedConfigs)
        {
            var expectedHeaders = group.Select(x => x.ExcelColumnHeader.Trim().ToLowerInvariant()).ToList();
            var matchCount = actualHeaders.Count(h => expectedHeaders.Contains(h.Trim().ToLowerInvariant()));

            if (matchCount > maxMatchCount)
            {
                maxMatchCount = matchCount;
                bestMatchTemplate = group.Key;
            }
        }

        return maxMatchCount > 0 ? bestMatchTemplate : string.Empty;
    }

    public async Task<bool> ValidateHeadersAsync(Stream stream, string templateCode, CancellationToken ct)
    {
        var configs = await GetTemplateConfigsAsync(templateCode, ct);
        var requiredHeaders = configs.Where(x => x.IsRequired)
                                     .Select(x => x.ExcelColumnHeader.Trim().ToLowerInvariant())
                                     .ToList();

        if (!requiredHeaders.Any())
            return true;

        var actualHeaders = GetHeadersFromStream(stream, requiredHeaders).Select(x => x.Trim().ToLowerInvariant()).ToList();

        foreach (var requiredHeader in requiredHeaders)
        {
            if (!actualHeaders.Contains(requiredHeader))
            {
                return false;
            }
        }

        return true;
    }

    private List<string> GetHeadersFromStream(Stream stream, List<string>? expectedHeaders = null)
    {
        var headers = new List<string>();
        try
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            
            using var workbook = new XLWorkbook(stream);
            
            IXLWorksheet? bestSheet = null;
            int maxMatchCount = 0;
            var expectedSet = expectedHeaders?.Select(h => h.Trim().ToLowerInvariant()).ToHashSet();

            foreach (var ws in workbook.Worksheets)
            {
                var firstRow = ws.Row(1);
                var lastCellUsed = firstRow.LastCellUsed();
                if (lastCellUsed == null) continue;

                int matchCount = 0;
                var sheetHeaders = new List<string>();
                for (int i = 1; i <= lastCellUsed.Address.ColumnNumber; i++)
                {
                    var cellValue = firstRow.Cell(i).GetString();
                    sheetHeaders.Add(cellValue ?? string.Empty);
                    if (expectedSet != null && !string.IsNullOrEmpty(cellValue))
                    {
                        if (expectedSet.Contains(cellValue.Trim().ToLowerInvariant()))
                        {
                            matchCount++;
                        }
                    }
                }

                // Nếu không truyền expectedHeaders, chọn Sheet đầu tiên có dữ liệu
                if (expectedSet == null)
                {
                    if (sheetHeaders.Any(h => !string.IsNullOrEmpty(h)))
                    {
                        bestSheet = ws;
                        headers = sheetHeaders;
                        break;
                    }
                }
                else
                {
                    if (matchCount > maxMatchCount)
                    {
                        maxMatchCount = matchCount;
                        bestSheet = ws;
                        headers = sheetHeaders;
                    }
                }
            }

            // Fallback nếu không tìm thấy Sheet nào khớp
            if (bestSheet == null)
            {
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws != null)
                {
                    var firstRow = ws.Row(1);
                    var lastCellUsed = firstRow.LastCellUsed();
                    if (lastCellUsed != null)
                    {
                        for (int i = 1; i <= lastCellUsed.Address.ColumnNumber; i++)
                        {
                            headers.Add(firstRow.Cell(i).GetString() ?? string.Empty);
                        }
                    }
                }
            }
        }
        catch
        {
            // Ignore parse errors for headers
        }

        return headers;
    }

    /// <summary>
    /// Sinh file Excel mẫu gồm 2 sheet: "Hướng dẫn" và "Dữ liệu" cho template tương ứng.
    /// </summary>
    public async Task<byte[]> GenerateTemplateAsync(string templateCode, CancellationToken ct)
    {
        var configs = await GetTemplateConfigsAsync(templateCode, ct);

        if (configs == null || configs.Count == 0)
            throw new NotFoundException("cấu hình template", templateCode);

        using var workbook = new XLWorkbook();

        // ===== Sheet 1: Hướng dẫn =====
        var guideSheet = workbook.Worksheets.Add("Hướng dẫn");
        var titleCell = guideSheet.Cell(1, 1);
        titleCell.Value = $"HƯỚNG DẪN IMPORT LÀNG NGHỀ — TEMPLATE {templateCode} ({configs.Count} CỘT)";
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.FontSize = 14;

        var guideLines = new List<string>
        {
            string.Empty,
            "QUY TẮC CHUNG:",
            "- Không thay đổi tên cột ở dòng 1 của sheet \"Dữ liệu\".",
            "- Không để trống các cột bắt buộc (nền hồng nhạt ở sheet \"Dữ liệu\").",
            "- Mỗi dòng dữ liệu tương ứng với một làng nghề.",
            "- File hỗ trợ định dạng .xlsx, dung lượng tối đa 20MB.",
            string.Empty,
            "QUY TẮC DỮ LIỆU:",
            "- RegionId, ClassificationId, VillageTypeId, IndustryGroupId phải là ID nội bộ (số nguyên), KHÔNG phải mã dạng chuỗi.",
            "- Ngày công nhận định dạng YYYY-MM-DD (ví dụ: 2020-01-15).",
            "- Cột kiểu bool nhập TRUE/FALSE hoặc 1/0.",
            "- Cột kiểu số nhập số nguyên hoặc số thực, không kèm đơn vị.",
            string.Empty,
            "DANH SÁCH CỘT:",
        };

        // Liệt kê chi tiết từng cột
        foreach (var config in configs)
        {
            var required = config.IsRequired ? "[Bắt buộc]" : "[Tùy chọn]";
            var line = $"- {config.ExcelColumnHeader} ({config.FieldName}) — {config.DataType} {required}";
            if (!string.IsNullOrWhiteSpace(config.Description))
                line += $" — {config.Description}";
            guideLines.Add(line);
        }

        for (int i = 0; i < guideLines.Count; i++)
        {
            guideSheet.Cell(i + 2, 1).Value = guideLines[i];
        }
        guideSheet.Columns().AdjustToContents();
        // Cột A rộng cố định để dễ đọc nội dung hướng dẫn
        guideSheet.Column(1).Width = 80;

        // ===== Sheet 2: Dữ liệu =====
        var dataSheet = workbook.Worksheets.Add("Dữ liệu");

        // Dòng 1: header
        for (int i = 0; i < configs.Count; i++)
        {
            var config = configs[i];
            var cell = dataSheet.Cell(1, i + 1);
            cell.Value = config.ExcelColumnHeader;
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Fill.BackgroundColor = config.IsRequired ? XLColor.LightPink : XLColor.White;
        }

        dataSheet.Row(1).Height = 20;
        dataSheet.SheetView.FreezeRows(1);

        // 2 dòng dữ liệu mẫu (dòng 2 và 3)
        for (int row = 2; row <= 3; row++)
        {
            for (int i = 0; i < configs.Count; i++)
            {
                var config = configs[i];
                var cell = dataSheet.Cell(row, i + 1);
                cell.Value = BuildSampleValue(config, row);
            }
        }

        dataSheet.Columns().AdjustToContents();

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Sinh giá trị mẫu cho một ô dựa trên kiểu dữ liệu của cột.
    /// </summary>
    private static XLCellValue BuildSampleValue(ImportTemplateConfig config, int row)
    {
        var dataType = (config.DataType ?? string.Empty).Trim().ToLowerInvariant();
        var fieldName = (config.FieldName ?? string.Empty).Trim().ToLowerInvariant();

        // Cột ID số nguyên (RegionId, ClassificationId, VillageTypeId, IndustryGroupId...)
        if (dataType is "int" or "integer" or "long" or "int32" or "int64")
        {
            if (fieldName.Contains("region"))
                return 35;
            if (fieldName.Contains("classification"))
                return 1;
            if (fieldName.Contains("villagetype"))
                return 8;
            if (fieldName.Contains("industrygroup"))
                return 1;
            return row == 2 ? 1 : 2;
        }

        if (dataType is "bool" or "boolean")
            return false;

        if (dataType is "datetime" or "date")
            return "2020-01-15";

        if (dataType is "decimal" or "double" or "float" or "number")
            return 0;

        // string mặc định
        if (fieldName.Contains("code"))
            return row == 2 ? "LN001" : "LN002";
        if (fieldName.Contains("name"))
            return row == 2 ? "Làng nghề mẫu 1" : "Làng nghề mẫu 2";
        if (fieldName.Contains("address"))
            return "Xã mẫu, Tỉnh mẫu";

        return string.Empty;
    }
}

