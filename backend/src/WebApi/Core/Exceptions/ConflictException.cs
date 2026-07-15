// Defines application errors for conflicting resource states.
namespace WebApi.Core.Exceptions;

/// <summary>
/// Exception raised when a command conflicts with the current application state.
/// </summary>
public sealed class ConflictException(string code, string message) : AppException(code, message);