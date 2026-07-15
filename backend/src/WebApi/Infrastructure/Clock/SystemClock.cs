// Provides the production UTC clock implementation.
using WebApi.Application.Core.Clock;

namespace WebApi.Infrastructure.Clock;

/// <summary>
/// Clock adapter used by application services for auditable timestamps.
/// </summary>
internal sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}