// Validates expense entry update commands before domain objects are mutated.
using FluentValidation;
using Microsoft.Extensions.Options;
using WebApi.Application.Core.Configuration;

namespace WebApi.Application.ExpenseEntries;

/// <summary>
/// FluentValidation rules protecting the update-expense-entry use case boundary.
/// </summary>
internal sealed class UpdateExpenseEntryCommandValidator : AbstractValidator<UpdateExpenseEntryCommand>
{
    private const string FrenchPostalCodePattern = @"^\d{5}$";

    public UpdateExpenseEntryCommandValidator(IOptions<ExpenseRulesOptions> options)
    {
        var rules = options.Value;

        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.ExpenseDate).Must(date => date != default).WithMessage("Expense date is required.");
        RuleFor(command => command.Description).NotEmpty().MaximumLength(rules.DescriptionMaxLength);
        RuleFor(command => command.Amount).GreaterThan(0);
        RuleFor(command => command.MerchantName).NotEmpty();
        RuleFor(command => command.Street).NotEmpty();
        RuleFor(command => command.PostalCode)
            .NotEmpty()
            .Matches(FrenchPostalCodePattern)
            .WithMessage("Billing postal code must contain exactly 5 digits.");
        RuleFor(command => command.City).NotEmpty();
    }
}
