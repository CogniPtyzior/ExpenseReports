// Defines the clock port used to keep technical timestamps testable.
namespace WebApi.Application.Core.Clock;

/// <summary>
/// Provides the current UTC instant to application use cases.
/// </summary>
internal interface IClock
{
    DateTime UtcNow { get; }
}