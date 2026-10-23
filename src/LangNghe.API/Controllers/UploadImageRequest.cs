using Microsoft.AspNetCore.Http;

namespace LangNghe.API.Controllers;

public class UploadImageRequest
{
    public IFormFile File { get; set; } = null!;
    public string Category { get; set; } = string.Empty;
}