// Defines expense entry write use cases exposed by the application layer.
namespace WebApi.Application.ExpenseEntries;

/// <summary>
/// Application port for expense entry mutations.
/// </summary>
internal interface IExpenseEntryCommandService
{
    Task<ExpenseEntryResult> CreateAsync(CreateExpenseEntryCommand command, CancellationToken cancellationToken);

    Task<ExpenseEntryResult> UpdateAsync(UpdateExpenseEntryCommand command, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
