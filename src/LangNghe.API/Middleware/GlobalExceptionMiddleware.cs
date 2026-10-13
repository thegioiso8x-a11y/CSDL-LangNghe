using System.Net;
using System.Text.Json;
using LangNghe.Application.Common;
using LangNghe.Domain.Exceptions;

namespace LangNghe.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Đã xảy ra lỗi không mong muốn.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = HttpStatusCode.InternalServerError;
        var response = ApiResponse<object>.Fail("Lỗi hệ thống, vui lòng liên hệ quản trị");

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.UnprocessableEntity;
                var errors = validationException.Errors.Values.SelectMany(x => x).ToList();
                response = ApiResponse<object>.Fail(errors);
                break;
                
            case BusinessException businessException:
                statusCode = HttpStatusCode.BadRequest;
                response = ApiResponse<object>.Fail(businessException.Message);
                break;
                
            case NotFoundException notFoundException:
                statusCode = HttpStatusCode.NotFound;
                response = ApiResponse<object>.Fail(notFoundException.Message);
                break;
                
            case UnauthorizedException unauthorizedException:
                statusCode = HttpStatusCode.Unauthorized;
                response = ApiResponse<object>.Fail(unauthorizedException.Message);
                break;
                
            case ForbiddenException forbiddenException:
                statusCode = HttpStatusCode.Forbidden;
                response = ApiResponse<object>.Fail(forbiddenException.Message);
                break;
        }

        context.Response.StatusCode = (int)statusCode;
        
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
