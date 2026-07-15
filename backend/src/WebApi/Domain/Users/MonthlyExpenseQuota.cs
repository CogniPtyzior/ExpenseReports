// Defines a validated monthly expense quota for users.
using WebApi.Core.Exceptions;

namespace WebApi.Domain.Users;

/// <summary>
/// Value object representing the maximum number of expenses a user can have per calendar month.
/// </summary>
internal readonly record struct MonthlyExpenseQuota
{
    private MonthlyExpenseQuota(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static MonthlyExpenseQuota Create(int value)
    {
        if (value <= 0)
        {
            throw new DomainException("user.monthly_quota_invalid", "Monthly expense quota must be positive.");
        }

        return new MonthlyExpenseQuota(value);
    }
}