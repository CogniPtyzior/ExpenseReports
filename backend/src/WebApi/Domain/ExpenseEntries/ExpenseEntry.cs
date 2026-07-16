// Defines expense entries and their month-bound lifecycle rules.
using WebApi.Core.Exceptions;
using WebApi.Domain.ExpenseReports;

namespace WebApi.Domain.ExpenseEntries;

/// <summary>
/// Entity representing one expense line attached to an expense report.
/// </summary>
internal sealed class ExpenseEntry
{
    private ExpenseEntry()
    {
        Description = null!;
        Amount = null!;
        BillingAddress = null!;
    }

    private ExpenseEntry(
        Guid id,
        Guid expenseReportId,
        CalendarMonth reportPeriod,
        DateOnly expenseDate,
        ExpenseDescription description,
        Money amount,
        BillingAddress billingAddress,
        DateTime createdAtUtc)
    {
        Id = id;
        ExpenseReportId = expenseReportId;
        ReportYear = reportPeriod.Year;
        ReportMonth = reportPeriod.Month;
        ExpenseDate = EnsureDateBelongsToReportMonth(expenseDate, reportPeriod);
        Description = description;
        Amount = amount;
        BillingAddress = billingAddress;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid ExpenseReportId { get; private set; }
    public int ReportYear { get; private set; }
    public int ReportMonth { get; private set; }
    public DateOnly ExpenseDate { get; private set; }
    public ExpenseDescription Description { get; private set; }
    public Money Amount { get; private set; }
    public BillingAddress BillingAddress { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }
    public CalendarMonth ReportPeriod => CalendarMonth.Create(ReportYear, ReportMonth);

    public static ExpenseEntry Create(
        Guid id,
        Guid expenseReportId,
        CalendarMonth reportPeriod,
        DateOnly expenseDate,
        ExpenseDescription description,
        Money amount,
        BillingAddress billingAddress,
        DateTime createdAtUtc)
    {
        if (expenseReportId == Guid.Empty)
        {
            throw new DomainException("expense_entry.report_required", "Expense report is required.");
        }

        if (reportPeriod is null)
        {
            throw new DomainException("expense_entry.report_period_required", "Expense report period is required.");
        }

        if (description is null)
        {
            throw new DomainException("expense_entry.description_required", "Description is required.");
        }

        if (amount is null)
        {
            throw new DomainException("expense_entry.amount_required", "Amount is required.");
        }

        if (billingAddress is null)
        {
            throw new DomainException("expense_entry.billing_address_required", "Billing address is required.");
        }

        return new ExpenseEntry(id, expenseReportId, reportPeriod, expenseDate, description, amount, billingAddress, createdAtUtc);
    }

    public void Update(
        DateOnly expenseDate,
        ExpenseDescription description,
        Money amount,
        BillingAddress billingAddress,
        DateTime updatedAtUtc)
    {
        if (description is null)
        {
            throw new DomainException("expense_entry.description_required", "Description is required.");
        }

        if (amount is null)
        {
            throw new DomainException("expense_entry.amount_required", "Amount is required.");
        }

        if (billingAddress is null)
        {
            throw new DomainException("expense_entry.billing_address_required", "Billing address is required.");
        }

        ExpenseDate = EnsureDateBelongsToReportMonth(expenseDate, ReportPeriod);
        Description = description;
        Amount = amount;
        BillingAddress = billingAddress;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void SoftDelete(DateTime deletedAtUtc)
    {
        IsDeleted = true;
        DeletedAtUtc = deletedAtUtc;
        UpdatedAtUtc = deletedAtUtc;
    }

    private static DateOnly EnsureDateBelongsToReportMonth(DateOnly expenseDate, CalendarMonth reportPeriod)
    {
        if (expenseDate.Year != reportPeriod.Year || expenseDate.Month != reportPeriod.Month)
        {
            throw new DomainException(
                "expense_entry.date_outside_report_month",
                "Expense date must belong to the expense report month.");
        }

        return expenseDate;
    }
}
