// Defines configurable expense rule values used by application workflows.
using System.ComponentModel.DataAnnotations;

namespace WebApi.Application.Core.Configuration;

/// <summary>
/// Options for simple expense rule thresholds that must stay aligned with README requirements.
/// </summary>
internal sealed class ExpenseRulesOptions
{
    public const string SectionName = "ExpenseRules";

    [Range(1, 500)]
    public int DescriptionMaxLength { get; init; } = 50;

    [Range(1, 100)]
    public int ExpenseEntriesPageSize { get; init; } = 5;

    [Range(1, 100)]
    public int DefaultMonthlyExpenseQuota { get; init; } = 5;

    [Required]
    public string SupportedCurrency { get; init; } = "EUR";
}
