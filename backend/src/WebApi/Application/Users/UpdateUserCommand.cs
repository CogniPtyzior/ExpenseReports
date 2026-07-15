// Defines the input contract used to update users from application use cases.
namespace WebApi.Application.Users;

/// <summary>
/// Command carrying editable user profile data.
/// </summary>
internal sealed record UpdateUserCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Street,
    string PostalCode,
    string City,
    int MonthlyExpenseQuota);