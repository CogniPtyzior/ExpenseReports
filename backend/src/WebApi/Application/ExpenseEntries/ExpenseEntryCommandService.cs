// Implements expense entry write use cases and their consistency checks.
using FluentValidation;
using WebApi.Application.Core.Clock;
using WebApi.Application.Core.Observability;
using WebApi.Application.Core.Persistence;
using WebApi.Application.ExpenseReports;
using WebApi.Application.Users;
using WebApi.Core.Exceptions;
using WebApi.Domain.ExpenseEntries;

namespace WebApi.Application.ExpenseEntries;

/// <summary>
/// Coordinates expense entry mutations while keeping month and value invariants in domain objects.
/// </summary>
internal sealed class ExpenseEntryCommandService(
    IExpenseEntryRepository entries,
    IExpenseReportRepository reports,
    IUserRepository users,
    IValidator<CreateExpenseEntryCommand> createValidator,
    IValidator<UpdateExpenseEntryCommand> updateValidator,
    IClock clock,
    IUnitOfWork unitOfWork,
    IApplicationLogger logger) : IExpenseEntryCommandService
{
    public async Task<ExpenseEntryResult> CreateAsync(CreateExpenseEntryCommand command, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(command, cancellationToken);

        return await unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
            await CreateInTransactionAsync(command, transactionCancellationToken), cancellationToken);
    }

    private async Task<ExpenseEntryResult> CreateInTransactionAsync(
        CreateExpenseEntryCommand command,
        CancellationToken cancellationToken)
    {
        var report = await reports.FindByIdForUpdateAsync(command.ExpenseReportId, cancellationToken)
            ?? throw new NotFoundException("expense_report.not_found", "Expense report was not found.");

        var user = await users.FindByIdAsync(report.UserId, cancellationToken)
            ?? throw new NotFoundException("user.not_found", "User was not found.");
        var activeEntryCount = await entries.CountActiveByReportAsync(report.Id, cancellationToken);
        if (activeEntryCount >= user.MonthlyExpenseQuota.Value)
        {
            throw new ConflictException("expense_entry.monthly_quota_reached", "Monthly expense quota has been reached.");
        }

        var entry = ExpenseEntry.Create(
            Guid.NewGuid(),
            report.Id,
            report.Period,
            command.ExpenseDate,
            ExpenseDescription.Create(command.Description),
            Money.Create(command.Amount, Currency.Eur),
            BillingAddress.Create(command.MerchantName, command.Street, command.PostalCode, command.City),
            clock.UtcNow);

        await entries.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.Information("Expense entry {ExpenseEntryId} was created.", entry.Id);

        return ExpenseEntryMapper.ToResult(entry);
    }

    public async Task<ExpenseEntryResult> UpdateAsync(UpdateExpenseEntryCommand command, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(command, cancellationToken);

        var entry = await entries.FindActiveByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("expense_entry.not_found", "Expense entry was not found.");

        entry.Update(
            command.ExpenseDate,
            ExpenseDescription.Create(command.Description),
            Money.Create(command.Amount, Currency.Eur),
            BillingAddress.Create(command.MerchantName, command.Street, command.PostalCode, command.City),
            clock.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.Information("Expense entry {ExpenseEntryId} was updated.", entry.Id);

        return ExpenseEntryMapper.ToResult(entry);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            throw new ValidationException("Expense entry id is required.");
        }

        var entry = await entries.FindActiveByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("expense_entry.not_found", "Expense entry was not found.");

        entry.SoftDelete(clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.Information("Expense entry {ExpenseEntryId} was logically deleted.", entry.Id);
    }
}
