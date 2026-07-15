// Defines the observability port used by application use cases.
namespace WebApi.Application.Core.Observability;

/// <summary>
/// Logs application-level events without binding use cases to a logging framework.
/// </summary>
internal interface IApplicationLogger
{
    void Information(string message, params object[] args);
}