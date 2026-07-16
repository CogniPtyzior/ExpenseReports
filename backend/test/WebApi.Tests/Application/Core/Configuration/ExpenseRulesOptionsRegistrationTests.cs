// Covers startup binding and validation for expense rule options.
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebApi.Application.Core.Configuration;

namespace WebApi.Tests.Application.Core.Configuration;

public sealed class ExpenseRulesOptionsRegistrationTests
{
    [Fact]
    public void Expense_rules_options_are_bound_from_configuration()
    {
        var provider = CreateProvider(new Dictionary<string, string?>
        {
            ["ExpenseRules:DescriptionMaxLength"] = "50",
            ["ExpenseRules:ExpenseEntriesPageSize"] = "5",
            ["ExpenseRules:DefaultMonthlyExpenseQuota"] = "5",
            ["ExpenseRules:SupportedCurrency"] = "EUR"
        });

        var options = provider.GetRequiredService<IOptions<ExpenseRulesOptions>>().Value;

        Assert.Equal(50, options.DescriptionMaxLength);
        Assert.Equal(5, options.ExpenseEntriesPageSize);
        Assert.Equal(5, options.DefaultMonthlyExpenseQuota);
        Assert.Equal("EUR", options.SupportedCurrency);
    }

    [Fact]
    public void Expense_rules_options_reject_unsupported_currency()
    {
        var provider = CreateProvider(new Dictionary<string, string?>
        {
            ["ExpenseRules:DescriptionMaxLength"] = "50",
            ["ExpenseRules:ExpenseEntriesPageSize"] = "5",
            ["ExpenseRules:DefaultMonthlyExpenseQuota"] = "5",
            ["ExpenseRules:SupportedCurrency"] = "USD"
        });

        Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<ExpenseRulesOptions>>().Value);
    }

    private static ServiceProvider CreateProvider(Dictionary<string, string?> values)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
        var services = new ServiceCollection();
        services.AddExpenseRulesOptions(configuration);
        return services.BuildServiceProvider(validateScopes: true);
    }
}