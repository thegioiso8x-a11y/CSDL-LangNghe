using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Domain.Entities;

namespace LangNghe.Application.Features.Import.Interfaces;

/// <summary>
/// Interface cho service xử lý template excel
/// </summary>
public interface IExcelTemplateService
{
    Task<List<ImportTemplateConfig>> GetTemplateConfigsAsync(string templateCode, CancellationToken ct);
    Task<string> DetectTemplateAsync(Stream stream, CancellationToken ct);
    Task<bool> ValidateHeadersAsync(Stream stream, string templateCode, CancellationToken ct);
}
