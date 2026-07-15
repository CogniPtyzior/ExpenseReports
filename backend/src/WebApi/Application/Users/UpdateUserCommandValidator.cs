// Validates user update commands before domain objects are instantiated.
using FluentValidation;

namespace WebApi.Application.Users;

/// <summary>
/// FluentValidation rules protecting the update-user use case boundary.
/// </summary>
internal sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.LastName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Street).NotEmpty().MaximumLength(200);
        RuleFor(command => command.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(command => command.City).NotEmpty().MaximumLength(100);
        RuleFor(command => command.MonthlyExpenseQuota).GreaterThan(0);
    }
}