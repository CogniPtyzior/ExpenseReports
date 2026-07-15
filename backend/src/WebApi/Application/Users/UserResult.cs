// Defines the user shape returned by application query use cases.
namespace WebApi.Application.Users;

/// <summary>
/// Read model returned by user use cases without exposing persistence entities.
/// </summary>
internal sealed record UserResult(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Street,
    string PostalCode,
    string City,
    int MonthlyExpenseQuota,
    bool IsActive,
    bool CanBeAssignedToExpenseReport);