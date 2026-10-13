using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Features.Import.Dtos;

namespace LangNghe.Application.Features.Import.Interfaces;

/// <summary>
/// Interface cho service xử lý import excel
/// </summary>
public interface IExcelImportService
{
    Task<List<Dictionary<string, object?>>> ParseAsync(Stream stream, string templateCode, CancellationToken ct);

    Task<ImportResultDto> ImportAsync(
        Stream stream,
        string fileName,
        long fileSizeBytes,
        string templateCode,
        string importMode,
        CancellationToken ct);

    Task<List<ImportPreviewRowDto>> PreviewAsync(
        Guid batchId,
        CancellationToken ct);
}
