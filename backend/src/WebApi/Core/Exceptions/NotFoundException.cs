// Defines application errors for missing resources.
namespace WebApi.Core.Exceptions;

/// <summary>
/// Exception raised when a requested resource cannot be found or used.
/// </summary>
public sealed class NotFoundException(string code, string message) : AppException(code, message);