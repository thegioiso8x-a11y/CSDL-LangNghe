namespace LangNghe.API.Controllers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Common;
using LangNghe.Application.Features.Import.Commands;
using LangNghe.Application.Features.Import.Constants;
using LangNghe.Application.Features.Import.Dtos;
using LangNghe.Application.Features.Import.Interfaces;
using LangNghe.Application.Features.Import.Queries;
using LangNghe.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/imports")]
[ApiController]
[Authorize]
public class ImportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IExcelImportService _excelImportService;

    public ImportsController(IMediator mediator, IExcelImportService excelImportService)
    {
        _mediator = mediator;
        _excelImportService = excelImportService;
    }

    /// <summary>
    /// Tải lên và import dữ liệu từ file Excel
    /// </summary>
    [HttpPost("upload")]
    [Authorize(Policy = "ImportUpload")]
    public async Task<ActionResult<ApiResponse<ImportResultDto>>> Upload(
        [FromForm] UploadImportRequest request,
        CancellationToken ct)
    {
        var file = request.File;
        var templateCode = request.TemplateCode;
        var importMode = request.ImportMode;

        if (file == null || file.Length == 0)
            throw new BusinessException("File không được trống");

        if (file.Length > 20 * 1024 * 1024) // 20 MB
            throw new BusinessException("Kích thước file không được vượt quá 20MB");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".xls")
            throw new BusinessException("Chỉ hỗ trợ file Excel (.xlsx, .xls)");

        if (templateCode != ImportTemplateCode.V1_FULL &&
            templateCode != ImportTemplateCode.V1_MINIMAL &&
            templateCode != ImportTemplateCode.MONTHLY)
            throw new BusinessException("Template code không hợp lệ");

        if (importMode != ImportMode.MERGE &&
            importMode != ImportMode.REPLACE_FULL &&
            importMode != ImportMode.INSERT_ONLY &&
            importMode != ImportMode.UPDATE_ONLY)
            throw new BusinessException("Import mode không hợp lệ");

        using var stream = file.OpenReadStream();
        var result = await _excelImportService.ImportAsync(stream, file.FileName, file.Length, templateCode, importMode, ct);
        
        return Ok(ApiResponse<ImportResultDto>.Ok(result, "Import thành công"));
    }

    /// <summary>
    /// Xem trước dữ liệu đã import (lấy các dòng có lỗi)
    /// </summary>
    [HttpGet("{batchId:guid}/preview")]
    [Authorize(Policy = "ImportUpload")]
    public async Task<ActionResult<ApiResponse<List<ImportPreviewRowDto>>>> Preview(Guid batchId, CancellationToken ct)
    {
        var result = await _excelImportService.PreviewAsync(batchId, ct);
        return Ok(ApiResponse<List<ImportPreviewRowDto>>.Ok(result));
    }

    /// <summary>
    /// Lấy danh sách lịch sử import
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "ImportUpload")]
    public async Task<ActionResult<ApiResponse<PagedResult<ImportResultDto>>>> GetHistory(
        [FromQuery] ImportFilterDto filter, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetImportHistoryListQuery(filter), ct);
        return Ok(ApiResponse<PagedResult<ImportResultDto>>.Ok(result));
    }

    /// <summary>
    /// Rollback 1 batch (chỉ đánh dấu, không xóa dữ liệu)
    /// </summary>
    [HttpPost("{batchId:guid}/rollback")]
    [Authorize(Policy = "ImportRollback")]
    public async Task<ActionResult<ApiResponse<bool>>> Rollback(Guid batchId, CancellationToken ct)
    {
        var result = await _mediator.Send(new RollbackImportCommand(batchId), ct);
        return Ok(ApiResponse<bool>.Ok(result, "Rollback thành công"));
    }
}