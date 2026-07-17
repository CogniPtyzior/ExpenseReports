// Covers expense entry application use cases with fake ports.
using FluentValidation;
using Microsoft.Extensions.Options;
using WebApi.Application.Core.Clock;
using WebApi.Application.Core.Configuration;
using WebApi.Application.Core.Observability;
using WebApi.Application.Core.Persistence;
using WebApi.Application.ExpenseEntries;
using WebApi.Application.ExpenseReports;
using WebApi.Application.Users;
using WebApi.Core.Exceptions;
using WebApi.Domain.ExpenseEntries;
using WebApi.Domain.ExpenseReports;
using WebApi.Domain.Users;

namespace WebApi.Tests.Application.ExpenseEntries;

public sealed class ExpenseEntryServiceTests
{
    private static readonly DateTime Now = new(2025, 10, 15, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Create_entry_for_existing_report_persists_and_commits()
    {
        var fixture = new Fixture();
        var report = fixture.AddReport();
        var service = fixture.CreateCommandService();

        var result = await service.CreateAsync(CreateCommand(report.Id), CancellationToken.None);

        Assert.Single(fixture.EntryRepository.Entries);
        Assert.Equal(report.Id, result.ExpenseReportId);
        Assert.Equal(new DateOnly(2025, 10, 15), result.ExpenseDate);
        Assert.Equal("EUR", result.Currency);
        Assert.Equal(1, fixture.UnitOfWork.SaveCount);
        Assert.Equal(1, fixture.ReportRepository.LockCount);
    }

    [Fact]
    public async Task Create_entry_rejects_missing_report()
    {
        var fixture = new Fixture();
        var service = fixture.CreateCommandService();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateAsync(CreateCommand(Guid.NewGuid()), CancellationToken.None));

        Assert.Equal("expense_report.not_found", exception.Code);
    }

    [Fact]
    public async Task Create_entry_rejects_date_outside_report_month()
    {
        var fixture = new Fixture();
        var report = fixture.AddReport();
        var service = fixture.CreateCommandService();

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            service.CreateAsync(CreateCommand(report.Id) with
            {
                ExpenseDate = new DateOnly(2025, 11, 1)
            }, CancellationToken.None));

        Assert.Equal("expense_entry.date_outside_report_month", exception.Code);
    }

    [Fact]
    public async Task Create_entry_rejects_when_monthly_quota_is_reached()
    {
        var fixture = new Fixture();
        var user = fixture.AddUser(monthlyQuota: 1);
        var report = fixture.AddReport(user);
        fixture.AddEntry(report);
        var service = fixture.CreateCommandService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(CreateCommand(report.Id), CancellationToken.None));

        Assert.Equal("expense_entry.monthly_quota_reached", exception.Code);
        Assert.Equal(0, fixture.UnitOfWork.SaveCount);
        Assert.Equal(1, fixture.ReportRepository.LockCount);
    }

    [Fact]
    public async Task Create_entry_ignores_soft_deleted_entries_when_checking_quota()
    {
        var fixture = new Fixture();
        var user = fixture.AddUser(monthlyQuota: 1);
        var report = fixture.AddReport(user);
        var deletedEntry = fixture.AddEntry(report);
        deletedEntry.SoftDelete(Now);
        var service = fixture.CreateCommandService();

        var result = await service.CreateAsync(CreateCommand(report.Id), CancellationToken.None);

        Assert.Equal(report.Id, result.ExpenseReportId);
        Assert.Equal(1, fixture.UnitOfWork.SaveCount);
        Assert.Equal(1, fixture.ReportRepository.LockCount);
    }

    [Fact]
    public async Task Update_entry_keeps_entry_on_its_existing_report_month()
    {
        var fixture = new Fixture();
        var report = fixture.AddReport();
        var entry = fixture.AddEntry(report);
        var service = fixture.CreateCommandService();

        var result = await service.UpdateAsync(UpdateCommand(entry.Id), CancellationToken.None);

        Assert.Equal("Lunch with client", result.Description);
        Assert.Equal(1, fixture.UnitOfWork.SaveCount);
        Assert.NotNull(entry.UpdatedAtUtc);
    }

    [Fact]
    public async Task Update_entry_rejects_date_outside_existing_report_month()
    {
        var fixture = new Fixture();
        var report = fixture.AddReport();
        var entry = fixture.AddEntry(report);
        var service = fixture.CreateCommandService();

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            service.UpdateAsync(UpdateCommand(entry.Id) with
            {
                ExpenseDate = new DateOnly(2025, 9, 30)
            }, CancellationToken.None));

        Assert.Equal("expense_entry.date_outside_report_month", exception.Code);
    }

    [Fact]
    public async Task Update_missing_entry_throws_not_found()
    {
        var fixture = new Fixture();
        var service = fixture.CreateCommandService();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateAsync(UpdateCommand(Guid.NewGuid()), CancellationToken.None));

        Assert.Equal("expense_entry.not_found", exception.Code);
    }

    [Fact]
    public async Task Delete_entry_marks_it_as_deleted_and_commits()
    {
        var fixture = new Fixture();
        var report = fixture.AddReport();
        var entry = fixture.AddEntry(report);
        var service = fixture.CreateCommandService();

        await service.DeleteAsync(entry.Id, CancellationToken.None);

        Assert.True(entry.IsDeleted);
        Assert.Equal(Now, entry.DeletedAtUtc);
        Assert.Equal(1, fixture.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Delete_missing_entry_throws_not_found()
    {
        var fixture = new Fixture();
        var service = fixture.CreateCommandService();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.DeleteAsync(Guid.NewGuid(), CancellationToken.None));

        Assert.Equal("expense_entry.not_found", exception.Code);
    }

    [Fact]
    public async Task Invalid_create_command_is_rejected_before_persistence()
    {
        var fixture = new Fixture();
        var report = fixture.AddReport();
        var service = fixture.CreateCommandService();

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(CreateCommand(report.Id) with
            {
                Description = string.Empty
            }, CancellationToken.None));

        Assert.Empty(fixture.EntryRepository.Entries);
    }

    private static CreateExpenseEntryCommand CreateCommand(Guid reportId)
    {
        return new CreateExpenseEntryCommand(
            reportId,
            new DateOnly(2025, 10, 15),
            "Restaurant",
            42.50m,
            "Cafe Central",
            "1 Rue des Notes",
            "75001",
            "Paris");
    }

    private static UpdateExpenseEntryCommand UpdateCommand(Guid id)
    {
        return new UpdateExpenseEntryCommand(
            id,
            new DateOnly(2025, 10, 16),
            "Lunch with client",
            55.10m,
            "Cafe Central",
            "1 Rue des Notes",
            "75001",
            "Paris");
    }

    private sealed class Fixture
    {
        public FakeExpenseEntryRepository EntryRepository { get; } = new();
        public FakeUserRepository UserRepository { get; } = new();
        public FakeExpenseReportRepository ReportRepository { get; } = new();
        public FakeUnitOfWork UnitOfWork { get; } = new();

        public ExpenseEntryCommandService CreateCommandService()
        {
            var options = Options.Create(new ExpenseRulesOptions());

            return new ExpenseEntryCommandService(
                EntryRepository,
                ReportRepository,
                UserRepository,
                new CreateExpenseEntryCommandValidator(options),
                new UpdateExpenseEntryCommandValidator(options),
                new FixedClock(),
                UnitOfWork,
                new NullApplicationLogger());
        }

        public User AddUser(int monthlyQuota = 5)
        {
            var user = User.Create(
                Guid.NewGuid(),
                PersonName.Create("Juste", "Leblanc"),
                PostalAddress.Create("1 Rue des Notes", "75001", "Paris"),
                MonthlyExpenseQuota.Create(monthlyQuota),
                Now);
            UserRepository.Users.Add(user);
            return user;
        }

        public ExpenseReport AddReport(User? user = null)
        {
            user ??= AddUser();
            var report = ExpenseReport.Create(
                Guid.NewGuid(),
                user.Id,
                user.Name.FullName,
                CalendarMonth.Create(2025, 10),
                Now);
            ReportRepository.Reports.Add(report);
            return report;
        }

        public ExpenseEntry AddEntry(ExpenseReport report)
        {
            var entry = ExpenseEntry.Create(
                Guid.NewGuid(),
                report.Id,
                report.Period,
                new DateOnly(2025, 10, 15),
                ExpenseDescription.Create("Restaurant"),
                Money.Create(42.50m, Currency.Eur),
                BillingAddress.Create("Cafe Central", "1 Rue des Notes", "75001", "Paris"),
                Now);
            EntryRepository.Entries.Add(entry);
            return entry;
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<User> Users { get; } = [];

        public Task<IReadOnlyCollection<User>> ListAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<User>>(Users.Where(user => !user.IsDeleted).ToArray());
        }

        public Task<IReadOnlyCollection<User>> ListAssignableAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<User>>(Users.Where(user => user.CanBeAssignedToExpenseReport).ToArray());
        }

        public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Users.FirstOrDefault(user => user.Id == id && !user.IsDeleted));
        }

        public Task AddAsync(User user, CancellationToken cancellationToken)
        {
            Users.Add(user);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeExpenseEntryRepository : IExpenseEntryRepository
    {
        public List<ExpenseEntry> Entries { get; } = [];

        public Task<IReadOnlyCollection<ExpenseEntry>> ListActiveByReportAsync(Guid expenseReportId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<ExpenseEntry>>(Entries
                .Where(entry => entry.ExpenseReportId == expenseReportId && !entry.IsDeleted)
                .ToArray());
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

        public int LockCount { get; private set; }

        public Task<ExpenseReport?> FindByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
        {
            LockCount++;
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
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public async Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<CancellationToken, Task<TResult>> operation,
            CancellationToken cancellationToken)
        {
            return await operation(cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock : IClock
    {
        public DateTime UtcNow => Now;
    }

    private sealed class NullApplicationLogger : IApplicationLogger
    {
        public void Information(string message, params object[] args)
        {
        }
    }
}
