// Validates expense entry creation commands before domain objects are instantiated.
using FluentValidation;
using Microsoft.Extensions.Options;
using WebApi.Application.Core.Configuration;

namespace WebApi.Application.ExpenseEntries;

/// <summary>
/// FluentValidation rules protecting the create-expense-entry use case boundary.
/// </summary>
internal sealed class CreateExpenseEntryCommandValidator : AbstractValidator<CreateExpenseEntryCommand>
{
    public CreateExpenseEntryCommandValidator(IOptions<ExpenseRulesOptions> options)
    {
        var rules = options.Value;

        RuleFor(command => command.ExpenseReportId).NotEmpty();
        RuleFor(command => command.ExpenseDate).Must(date => date != default).WithMessage("Expense date is required.");
        RuleFor(command => command.Description).NotEmpty().MaximumLength(rules.DescriptionMaxLength);
        RuleFor(command => command.Amount).GreaterThan(0);
        RuleFor(command => command.MerchantName).NotEmpty();
        RuleFor(command => command.Street).NotEmpty();
        RuleFor(command => command.PostalCode).NotEmpty();
        RuleFor(command => command.City).NotEmpty();
    }
}
