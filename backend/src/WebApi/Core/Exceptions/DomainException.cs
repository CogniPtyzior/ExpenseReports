// Defines domain-level exceptions raised by aggregates and value objects.
namespace WebApi.Core.Exceptions;

/// <summary>
/// Exception raised when a domain invariant is violated.
/// </summary>
public sealed class DomainException(string code, string message) : AppException(code, message);