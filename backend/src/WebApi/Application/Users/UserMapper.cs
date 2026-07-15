// Maps domain users to application read models.
using WebApi.Domain.Users;

namespace WebApi.Application.Users;

/// <summary>
/// Centralized mapper keeping endpoint code free from domain projection details.
/// </summary>
internal static class UserMapper
{
    public static UserResult ToResult(User user)
    {
        return new UserResult(
            user.Id,
            user.Name.FirstName,
            user.Name.LastName,
            user.Name.FullName,
            user.Address.Street,
            user.Address.PostalCode,
            user.Address.City,
            user.MonthlyExpenseQuota.Value,
            user.IsActive,
            user.CanBeAssignedToExpenseReport);
    }
}