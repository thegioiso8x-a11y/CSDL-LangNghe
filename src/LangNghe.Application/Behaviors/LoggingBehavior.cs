using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LangNghe.Application.Behaviors;

/// <summary>
/// Behavior dùng để ghi log thời gian thực thi của các MediatR request.
/// </summary>
/// <typeparam name="TRequest">Kiểu dữ liệu request</typeparam>
/// <typeparam name="TResponse">Kiểu dữ liệu trả về</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Bắt đầu xử lý Request: {Name}", requestName);
        var timer = Stopwatch.StartNew();

        var response = await next();

        timer.Stop();
        _logger.LogInformation("Hoàn thành Request: {Name} trong {ElapsedMilliseconds} ms", requestName, timer.ElapsedMilliseconds);

        return response;
    }
}
