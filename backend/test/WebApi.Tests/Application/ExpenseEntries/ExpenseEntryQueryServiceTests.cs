// Covers expense entry read use cases with fake ports.
using FluentValidation;
using Microsoft.Extensions.Options;
using WebApi.Application.Core.Configuration;
using WebApi.Application.ExpenseEntries;
using WebApi.Application.ExpenseReports;
using WebApi.Core.Exceptions;
using WebApi.Domain.ExpenseEntries;
using WebApi.Domain.ExpenseReports;

namespace WebApi.Tests.Application.ExpenseEntries;

public sealed class ExpenseEntryQueryServiceTests
{
    private static readonly DateTime Now = new(2025, 10, 15, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task List_by_report_returns_active_entries_with_configured_page_size()
    {
        var fixture = new Fixture(pageSize: 2);
        var report = fixture.AddReport();
        fixture.AddEntry(report, new DateOnly(2025, 10, 10), "A");
        fixture.AddEntry(report, new DateOnly(2025, 10, 11), "B");
        fixture.AddEntry(report, new DateOnly(2025, 10, 12), "C");
        var deleted = fixture.AddEntry(report, new DateOnly(2025, 10, 13), "Deleted");
        deleted.SoftDelete(Now);
        var service = fixture.CreateService();

        var result = await service.ListByReportAsync(report.Id, 2, CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal("C", item.Description);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task List_by_report_rejects_missing_report()
    {
        var service = new Fixture().CreateService();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.ListByReportAsync(Guid.NewGuid(), 1, CancellationToken.None));

        Assert.Equal("expense_report.not_found", exception.Code);
    }

    [Fact]
    public async Task List_by_report_rejects_invalid_page_number()
    {
        var fixture = new Fixture();
        var report = fixture.AddReport();
        var service = fixture.CreateService();

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.ListByReportAsync(report.Id, 0, CancellationToken.None));
    }

    private sealed class Fixture(int pageSize = 5)
    {
        private readonly FakeExpenseEntryRepository entries = new();
        private readonly FakeExpenseReportRepository reports = new();

        public ExpenseEntryQueryService CreateService()
        {
            return new ExpenseEntryQueryService(
                entries,
                reports,
                Options.Create(new ExpenseRulesOptions { ExpenseEntriesPageSize = pageSize }));
        }

        public ExpenseReport AddReport()
        {
            var report = ExpenseReport.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Juste Leblanc",
                CalendarMonth.Create(2025, 10),
                Now);
            reports.Reports.Add(report);
            return report;
        }

        public ExpenseEntry AddEntry(ExpenseReport report, DateOnly expenseDate, string description)
        {
            var entry = ExpenseEntry.Create(
                Guid.NewGuid(),
                report.Id,
                report.Period,
                expenseDate,
                ExpenseDescription.Create(description),
                Money.Create(25m, Currency.Eur),
                BillingAddress.Create("Cafe Paris", "1 Rue des Notes", "75001", "Paris"),
                Now);
            entries.Entries.Add(entry);
            return entry;
        }
    }

    private sealed class FakeExpenseEntryRepository : IExpenseEntryRepository
    {
        public List<ExpenseEntry> Entries { get; } = [];

        public Task<IReadOnlyCollection<ExpenseEntry>> ListActiveByReportAsync(
            Guid expenseReportId,
            CancellationToken cancellationToken)
        {
            return ListActiveByReportAsync(expenseReportId, 1, int.MaxValue, cancellationToken);
        }

        public Task<IReadOnlyCollection<ExpenseEntry>> ListActiveByReportAsync(
            Guid expenseReportId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<ExpenseEntry>>(Entries
                .Where(entry => entry.ExpenseReportId == expenseReportId && !entry.IsDeleted)
                .OrderBy(entry => entry.ExpenseDate)
                .ThenBy(entry => entry.CreatedAtUtc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToArray());
        }

        public Task<int> CountActiveByReportAsync(Guid expenseReportId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Entries.Count(entry => entry.ExpenseReportId == expenseReportId && !entry.IsDeleted));
        }

        public Task<ExpenseEntry?> FindActiveByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Entries.FirstOrDefault(entry => entry.Id == id && !entry.IsDeleted));
        }

        public Task AddAsync(ExpenseEntry entry, CancellationToken cancellationToken)
        {
            Entries.Add(entry);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeExpenseReportRepository : IExpenseReportRepository
    {
        public List<ExpenseReport> Reports { get; } = [];

        public Task<IReadOnlyCollection<ExpenseReport>> ListAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<ExpenseReport>>(Reports.ToArray());
        }

        public Task<ExpenseReport?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Reports.FirstOrDefault(report => report.Id == id));
        }

        public Task<ExpenseReport?> FindByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
        {
            return FindByIdAsync(id, cancellationToken);
        }

        public Task<bool> ExistsForUserAndMonthAsync(Guid userId, CalendarMonth period, CancellationToken cancellationToken)
        {
            return Task.FromResult(Reports.Any(report => report.UserId == userId
                && report.Period.Year == period.Year
                && report.Period.Month == period.Month));
        }

        public Task AddAsync(ExpenseReport report, CancellationToken cancellationToken)
        {
            Reports.Add(report);
            return Task.CompletedTask;
        }

        public void Remove(ExpenseReport report)
        {
            Reports.Remove(report);
        }
    }
}
