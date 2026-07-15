// Bridges the application logging port to Microsoft.Extensions.Logging.
using WebApi.Application.Core.Observability;

namespace WebApi.Infrastructure.Observability;

/// <summary>
/// Production logging adapter keeping use cases independent from a concrete logging framework.
/// </summary>
internal sealed class MicrosoftApplicationLogger<TCategory>(ILogger<TCategory> logger) : IApplicationLogger
{
    public void Information(string message, params object[] args)
    {
        logger.LogInformation(message, args);
    }
}