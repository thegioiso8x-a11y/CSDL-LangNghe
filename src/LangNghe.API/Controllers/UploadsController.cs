namespace LangNghe.API.Controllers;

using LangNghe.Application.Common;
using LangNghe.Application.Features.Uploads.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Endpoint upload ảnh.
/// </summary>
[ApiController]
[Route("api/v1/uploads")]
[Authorize]
public class UploadsController : ControllerBase
{
    private readonly IFileStorageService _fileStorage;

    public UploadsController(IFileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    /// <summary>
    /// Upload 1 ảnh. Trả về URL tương đối để FE lưu vào entity.
    /// </summary>
    /// <param name="request">Thông tin file và category</param>
    /// <param name="ct">Token hủy thao tác upload.</param>
    [HttpPost("image")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<string>>> UploadImage(
        [FromForm] UploadImageRequest request,
        CancellationToken ct)
    {
        var file = request.File;
        var category = request.Category;

        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("File rỗng."));

        var allowed = new[] { "villages", "products", "artisans" };
        if (!allowed.Contains(category))
            return BadRequest(ApiResponse<object>.Fail($"Category phải là: {string.Join(", ", allowed)}"));

        using var stream = file.OpenReadStream();
        var url = await _fileStorage.SaveImageAsync(stream, file.FileName, category, ct);

        return Ok(ApiResponse<string>.Ok(url, "Upload thành công"));
    }
}