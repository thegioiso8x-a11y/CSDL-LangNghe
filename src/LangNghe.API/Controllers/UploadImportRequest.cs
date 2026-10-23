using Microsoft.AspNetCore.Http;

namespace LangNghe.API.Controllers;

public class UploadImportRequest
{
    public IFormFile File { get; set; } = null!;
    public string TemplateCode { get; set; } = string.Empty;
    public string ImportMode { get; set; } = string.Empty;
}