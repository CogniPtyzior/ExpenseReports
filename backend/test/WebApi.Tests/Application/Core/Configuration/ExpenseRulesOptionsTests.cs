// Covers expense rule option defaults and startup validation constraints.
using System.ComponentModel.DataAnnotations;
using WebApi.Application.Core.Configuration;

namespace WebApi.Tests.Application.Core.Configuration;

public sealed class ExpenseRulesOptionsTests
{
    [Fact]
    public void Defaults_match_readme_requirements()
    {
        var options = new ExpenseRulesOptions();

        Assert.Equal(50, options.DescriptionMaxLength);
        Assert.Equal(5, options.ExpenseEntriesPageSize);
        Assert.Equal(5, options.DefaultMonthlyExpenseQuota);
        Assert.Equal("EUR", options.SupportedCurrency);
    }

    [Fact]
    public void Data_annotations_reject_invalid_thresholds()
    {
        var options = new ExpenseRulesOptions
        {
            DescriptionMaxLength = 0,
            ExpenseEntriesPageSize = 0,
            DefaultMonthlyExpenseQuota = 0,
            SupportedCurrency = string.Empty
        };

        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(ExpenseRulesOptions.DescriptionMaxLength)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(ExpenseRulesOptions.ExpenseEntriesPageSize)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(ExpenseRulesOptions.DefaultMonthlyExpenseQuota)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(ExpenseRulesOptions.SupportedCurrency)));
    }
}
