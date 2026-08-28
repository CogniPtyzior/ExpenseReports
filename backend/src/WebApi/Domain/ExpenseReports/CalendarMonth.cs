// Defines the calendar month value object used by expense reports.
using WebApi.Core.Exceptions;

namespace WebApi.Domain.ExpenseReports;

/// <summary>
/// Value object representing a valid calendar month without timezone semantics.
/// </summary>
internal sealed class CalendarMonth
{
    private static readonly string[] FrenchMonthNames =
    [
        "Janvier",
        "Fevrier",
        "Mars",
        "Avril",
        "Mai",
        "Juin",
        "Juillet",
        "Aout",
        "Septembre",
        "Octobre",
        "Novembre",
        "Decembre"
    ];

    private CalendarMonth()
    {
    }

    private CalendarMonth(int year, int month)
    {
        Year = year;
        Month = month;
    }

    public int Year { get; private set; }
    public int Month { get; private set; }
    public string FrenchLabel => $"{FrenchMonthNames[Month - 1]} {Year}";

    public static CalendarMonth Create(int year, int month)
    {
        var currentYear = DateTime.UtcNow.Year;
        if (year != currentYear)
        {
            throw new DomainException("expense_report.year_invalid", "Year must be the current year.");
        }

        if (month is < 1 or > 12)
        {
            throw new DomainException("expense_report.month_invalid", "Month must be between 1 and 12.");
        }

        return new CalendarMonth(year, month);
    }
}
