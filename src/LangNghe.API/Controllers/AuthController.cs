using LangNghe.Application.Common;
using LangNghe.Application.Features.Auth.Commands.ChangePassword;
using LangNghe.Application.Features.Auth.Commands.Login;
using LangNghe.Application.Features.Auth.Commands.RefreshToken;
using LangNghe.Application.Features.Auth.Commands.RevokeToken;
using LangNghe.Application.Features.Auth.DTOs;
using LangNghe.Application.Features.Auth.Queries.GetCaptcha;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LangNghe.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _mediator.Send(new LoginCommand(dto, ip), ct);
        return Ok(ApiResponse<LoginResponseDto>.Ok(result, "Đăng nhập thành công"));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto dto, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _mediator.Send(new RefreshTokenCommand(dto.RefreshToken, ip), ct);
        return Ok(ApiResponse<LoginResponseDto>.Ok(result, "Làm mới token thành công"));
    }

    [HttpPost("revoke")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<bool>>> RevokeToken([FromBody] RevokeTokenRequestDto dto, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _mediator.Send(new RevokeTokenCommand(dto.RefreshToken, ip), ct);
        return Ok(ApiResponse<bool>.Ok(result, "Thu hồi token thành công"));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordRequestDto dto, CancellationToken ct)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized(ApiResponse<bool>.Fail("Chưa đăng nhập"));
        }

        var result = await _mediator.Send(new ChangePasswordCommand(_currentUserService.UserId.Value, dto), ct);
        return Ok(ApiResponse<bool>.Ok(result, "Đổi mật khẩu thành công"));
    }

    [HttpGet("captcha")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<CaptchaResponseDto>>> GetCaptcha(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCaptchaQuery(), ct);
        return Ok(ApiResponse<CaptchaResponseDto>.Ok(result));
    }
}
