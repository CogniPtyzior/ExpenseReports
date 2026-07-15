// Defines the root exception type for expected application failures.
namespace WebApi.Core.Exceptions;

/// <summary>
/// Base exception for expected application errors that can be mapped to stable API responses.
/// </summary>
public abstract class AppException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}