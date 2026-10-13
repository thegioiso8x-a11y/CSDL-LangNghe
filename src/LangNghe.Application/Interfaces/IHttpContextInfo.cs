namespace LangNghe.Application.Interfaces;

/// <summary>
/// Cung cấp thông tin IP và UserAgent của client hiện tại từ HttpContext.
/// </summary>
public interface IHttpContextInfo
{
    /// <summary>
    /// Địa chỉ IP của client gửi yêu cầu.
    /// </summary>
    string? IpAddress { get; }

    /// <summary>
    /// Chuỗi User-Agent của client gửi yêu cầu.
    /// </summary>
    string? UserAgent { get; }
}
