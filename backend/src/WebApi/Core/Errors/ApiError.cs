// Defines the stable API error response shape shared by presentation concerns.
namespace WebApi.Core.Errors;

/// <summary>
/// Stable error contract returned by the HTTP layer when an application error must be exposed.
/// </summary>
public sealed record ApiError(string Code, string Message);