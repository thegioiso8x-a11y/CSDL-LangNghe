using FluentValidation;
using MediatR;
using ValidationException = LangNghe.Domain.Exceptions.ValidationException;

namespace LangNghe.Application.Behaviors;

/// <summary>
/// Behavior chặn giữa các request của MediatR để kiểm tra tính hợp lệ của dữ liệu bằng FluentValidation.
/// </summary>
/// <typeparam name="TRequest">Kiểu dữ liệu đầu vào</typeparam>
/// <typeparam name="TResponse">Kiểu dữ liệu trả về</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Count > 0)
            {
                var groupedErrors = failures
                    .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                    .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());

                throw new ValidationException(groupedErrors);
            }
        }

        return await next();
    }
}
