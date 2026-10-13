using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using LangNghe.Application.Common;
using LangNghe.Application.Features.Import.Interfaces;
using LangNghe.Domain.Entities;
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

        var actualHeaders = GetHeadersFromStream(stream);
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

        var actualHeaders = GetHeadersFromStream(stream).Select(x => x.Trim().ToLowerInvariant()).ToList();

        foreach (var requiredHeader in requiredHeaders)
        {
            if (!actualHeaders.Contains(requiredHeader))
            {
                return false;
            }
        }

        return true;
    }

    private List<string> GetHeadersFromStream(Stream stream)
    {
        var headers = new List<string>();
        try
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            
            if (worksheet != null)
            {
                var firstRow = worksheet.Row(1);
                var lastCellUsed = firstRow.LastCellUsed();
                
                if (lastCellUsed != null)
                {
                    int lastColNumber = lastCellUsed.Address.ColumnNumber;
                    for (int i = 1; i <= lastColNumber; i++)
                    {
                        var cellValue = firstRow.Cell(i).GetString();
                        headers.Add(cellValue ?? string.Empty);
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
}
