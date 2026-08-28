// Covers expense report aggregate and calendar month invariants.
using WebApi.Core.Exceptions;
using WebApi.Domain.ExpenseReports;

namespace WebApi.Tests.Domain.ExpenseReports;

public sealed class ExpenseReportTests
{
    private static int CurrentYear => DateTime.UtcNow.Year;
    private static readonly DateTime Now = new(2025, 10, 15, 8, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void Calendar_month_validates_month_range_and_formats_french_label()
    {
        var period = CalendarMonth.Create(CurrentYear, 10);

        Assert.Equal(CurrentYear, period.Year);
        Assert.Equal(10, period.Month);
        Assert.Equal($"Octobre {CurrentYear}", period.FrenchLabel);
        Assert.Throws<DomainException>(() => CalendarMonth.Create(CurrentYear - 1, 10));
        Assert.Throws<DomainException>(() => CalendarMonth.Create(CurrentYear, 13));
    }

    [Fact]
    public void Expense_report_title_is_computed_from_user_and_month()
    {
        var report = ExpenseReport.Create(Guid.NewGuid(), Guid.NewGuid(), " Juste Leblanc ", CalendarMonth.Create(CurrentYear, 10), Now);

        Assert.Equal($"Juste Leblanc - Octobre {CurrentYear}", report.Title);
        Assert.Equal(Now, report.CreatedAtUtc);
    }

    [Fact]
    public void Expense_report_requires_assigned_user_and_name()
    {
        Assert.Throws<DomainException>(() => ExpenseReport.Create(
            Guid.NewGuid(),
            Guid.Empty,
            "Juste Leblanc",
            CalendarMonth.Create(CurrentYear, 10),
            Now));
        Assert.Throws<DomainException>(() => ExpenseReport.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            " ",
            CalendarMonth.Create(CurrentYear, 10),
            Now));
    }
}