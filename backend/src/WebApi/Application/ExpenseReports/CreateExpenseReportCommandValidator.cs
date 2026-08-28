// Validates expense report creation commands before domain objects are instantiated.
using FluentValidation;

namespace WebApi.Application.ExpenseReports;

/// <summary>
/// FluentValidation rules protecting the create-expense-report use case boundary.
/// </summary>
internal sealed class CreateExpenseReportCommandValidator : AbstractValidator<CreateExpenseReportCommand>
{
    public CreateExpenseReportCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Year).Must(year =>
        {
            var currentYear = DateTime.UtcNow.Year;
            return year >= currentYear - 1 && year <= currentYear + 1;
        });
        RuleFor(command => command.Month).InclusiveBetween(1, 12);
    }
}