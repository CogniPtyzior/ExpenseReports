// Covers expense report application use cases with fake ports.
using FluentValidation;
using WebApi.Application.Core.Clock;
using WebApi.Application.Core.Observability;
using WebApi.Application.Core.Persistence;
using WebApi.Application.ExpenseReports;
using WebApi.Application.Users;
using WebApi.Core.Exceptions;
using WebApi.Domain.ExpenseReports;
using WebApi.Domain.Users;

namespace WebApi.Tests.Application.ExpenseReports;

public sealed class ExpenseReportServiceTests
{
    private static readonly DateTime Now = new(2025, 10, 15, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Create_report_for_active_user_persists_and_commits()
    {
        var fixture = new Fixture();
        var user = fixture.AddUser();
        var service = fixture.CreateCommandService();

        var result = await service.CreateAsync(new CreateExpenseReportCommand(user.Id, 2025, 10), CancellationToken.None);

        Assert.Single(fixture.ReportRepository.Reports);
        Assert.Equal("Juste Leblanc - Octobre 2025", result.Title);
        Assert.Equal(1, fixture.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Create_report_rejects_inactive_user()
    {
        var fixture = new Fixture();
        var user = fixture.AddUser();
        user.Deactivate(Now);
        var service = fixture.CreateCommandService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(new CreateExpenseReportCommand(user.Id, 2025, 10), CancellationToken.None));

        Assert.Equal("expense_report.user_not_assignable", exception.Code);
    }

    [Fact]
    public async Task Create_report_rejects_duplicate_user_month()
    {
        var fixture = new Fixture();
        var user = fixture.AddUser();
        fixture.ReportRepository.Reports.Add(ExpenseReport.Create(
            Guid.NewGuid(),
            user.Id,
            user.Name.FullName,
            CalendarMonth.Create(2025, 10),
            Now));
        var service = fixture.CreateCommandService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(new CreateExpenseReportCommand(user.Id, 2025, 10), CancellationToken.None));

        Assert.Equal("expense_report.already_exists", exception.Code);
    }

    [Fact]
    public async Task Query_missing_report_throws_not_found()
    {
        var service = new ExpenseReportQueryService(new FakeExpenseReportRepository());

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.GetAsync(Guid.NewGuid(), CancellationToken.None));

        Assert.Equal("expense_report.not_found", exception.Code);
    }

    private sealed class Fixture
    {
        public FakeUserRepository UserRepository { get; } = new();
        public FakeExpenseReportRepository ReportRepository { get; } = new();
        public FakeUnitOfWork UnitOfWork { get; } = new();

        public ExpenseReportCommandService CreateCommandService()
        {
            return new ExpenseReportCommandService(
                ReportRepository,
                UserRepository,
                new CreateExpenseReportCommandValidator(),
                new FixedClock(),
                UnitOfWork,
                new NullApplicationLogger());
        }

        public User AddUser()
        {
            var user = User.Create(
                Guid.NewGuid(),
                PersonName.Create("Juste", "Leblanc"),
                PostalAddress.Create("1 Rue des Notes", "75001", "Paris"),
                MonthlyExpenseQuota.Create(5),
                Now);
            UserRepository.Users.Add(user);
            return user;
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