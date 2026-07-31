namespace NLIP.Application.Common.Interfaces;

/// <summary>Testable clock abstraction — never call DateTime.UtcNow directly inside handlers.</summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
