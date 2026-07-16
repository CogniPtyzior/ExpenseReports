// Defines the expense report aggregate and its title rule.
using WebApi.Core.Exceptions;

namespace WebApi.Domain.ExpenseReports;

/// <summary>
/// Expense report aggregate representing one user's expenses for a calendar month.
/// </summary>
internal sealed class ExpenseReport
{
    private ExpenseReport()
    {
        AssignedUserFullName = string.Empty;
    }

    private ExpenseReport(Guid id, Guid userId, string assignedUserFullName, CalendarMonth period, DateTime createdAtUtc)
    {
        Id = id;
        UserId = userId;
        AssignedUserFullName = NormalizeAssignedUserFullName(assignedUserFullName);
        Year = period.Year;
        Month = period.Month;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string AssignedUserFullName { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public CalendarMonth Period => CalendarMonth.Create(Year, Month);
    public string Title => $"{AssignedUserFullName} - {Period.FrenchLabel}";

    public static ExpenseReport Create(
        Guid id,
        Guid userId,
        string assignedUserFullName,
        CalendarMonth period,
        DateTime createdAtUtc)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("expense_report.user_required", "Assigned user is required.");
        }

        return new ExpenseReport(id, userId, assignedUserFullName, period, createdAtUtc);
    }

    private static string NormalizeAssignedUserFullName(string assignedUserFullName)
    {
        if (string.IsNullOrWhiteSpace(assignedUserFullName))
        {
            throw new DomainException("expense_report.user_name_required", "Assigned user name is required.");
        }

        return assignedUserFullName.Trim();
    }
}