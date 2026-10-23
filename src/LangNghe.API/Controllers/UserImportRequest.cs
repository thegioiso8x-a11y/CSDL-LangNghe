using Microsoft.AspNetCore.Http;

namespace LangNghe.API.Controllers;

public class UserImportRequest
{
    public IFormFile File { get; set; } = null!;
    public string ImportMode { get; set; } = string.Empty;
    public bool SkipErrors { get; set; } = false;
}