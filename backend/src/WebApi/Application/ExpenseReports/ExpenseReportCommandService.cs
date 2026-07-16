// Implements expense report write use cases and their consistency checks.
using FluentValidation;
using WebApi.Application.Core.Clock;
using WebApi.Application.Core.Observability;
using WebApi.Application.Core.Persistence;
using WebApi.Application.Users;
using WebApi.Core.Exceptions;
using WebApi.Domain.ExpenseReports;

namespace WebApi.Application.ExpenseReports;

/// <summary>
/// Coordinates expense report mutations while keeping invariants in domain objects.
/// </summary>
internal sealed class ExpenseReportCommandService(
    IExpenseReportRepository reports,
    IUserRepository users,
    IValidator<CreateExpenseReportCommand> validator,
    IClock clock,
    IUnitOfWork unitOfWork,
    IApplicationLogger logger) : IExpenseReportCommandService
{
    public async Task<ExpenseReportResult> CreateAsync(CreateExpenseReportCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var user = await users.FindByIdAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException("user.not_found", "User was not found.");

        if (!user.CanBeAssignedToExpenseReport)
        {
            throw new ConflictException("expense_report.user_not_assignable", "User cannot be assigned to a new expense report.");
        }

        var period = CalendarMonth.Create(command.Year, command.Month);
        if (await reports.ExistsForUserAndMonthAsync(user.Id, period, cancellationToken))
        {
            throw new ConflictException("expense_report.already_exists", "An expense report already exists for this user and month.");
        }

        var report = ExpenseReport.Create(Guid.NewGuid(), user.Id, user.Name.FullName, period, clock.UtcNow);
        await reports.AddAsync(report, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.Information("Expense report {ExpenseReportId} was created.", report.Id);

        return ExpenseReportMapper.ToResult(report);
    }
}