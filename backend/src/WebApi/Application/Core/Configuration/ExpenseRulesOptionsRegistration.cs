// Registers and validates expense rule options from configuration.
using Microsoft.Extensions.Options;

namespace WebApi.Application.Core.Configuration;

/// <summary>
/// Centralizes expense rule option binding so startup and tests use the same validation path.
/// </summary>
internal static class ExpenseRulesOptionsRegistration
{
    public static IServiceCollection AddExpenseRulesOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ExpenseRulesOptions>()
            .Bind(configuration.GetSection(ExpenseRulesOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options => options.SupportedCurrency == "EUR", "Only EUR is supported by the current domain model.")
            .ValidateOnStart();

        return services;
    }
}