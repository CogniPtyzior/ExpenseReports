// Centralizes application-layer service registrations.
using FluentValidation;
using WebApi.Application.ExpenseEntries;
using WebApi.Application.ExpenseReports;
using WebApi.Application.Users;

namespace WebApi.Application;

/// <summary>
/// Registers application services and validators.
/// </summary>
internal static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>(includeInternalTypes: true);
        services.AddScoped<IUserCommandService, UserCommandService>();
        services.AddScoped<IUserQueryService, UserQueryService>();
        services.AddScoped<IExpenseEntryCommandService, ExpenseEntryCommandService>();
        services.AddScoped<IExpenseReportCommandService, ExpenseReportCommandService>();
        services.AddScoped<IExpenseReportQueryService, ExpenseReportQueryService>();

        return services;
    }
}