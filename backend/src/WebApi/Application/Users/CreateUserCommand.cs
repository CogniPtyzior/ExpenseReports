// Defines the input contract used to create users from application use cases.
namespace WebApi.Application.Users;

/// <summary>
/// Command carrying the business data required to create an assignable user.
/// </summary>
internal sealed record CreateUserCommand(
    string FirstName,
    string LastName,
    string Street,
    string PostalCode,
    string City,
    int MonthlyExpenseQuota);