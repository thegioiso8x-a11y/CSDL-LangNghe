namespace LangNghe.Domain.Interfaces;

/// <summary>
/// Abstraction cho DateTime để dễ dàng mock trong Unit Test.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Trả về thời gian hiện tại theo UTC.
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Trả về thời gian hiện tại ở Local (Server).
    /// </summary>
    DateTime Now { get; }
}