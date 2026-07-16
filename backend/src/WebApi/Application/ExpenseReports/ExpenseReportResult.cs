// Defines the expense report read model returned by application use cases.
namespace WebApi.Application.ExpenseReports;

/// <summary>
/// Read model exposing an expense report without leaking persistence concerns.
/// </summary>
internal sealed record ExpenseReportResult(
    Guid Id,
    Guid UserId,
    string AssignedUserFullName,
    int Year,
    int Month,
    string Title,
    DateTime CreatedAtUtc);