// Exposes expense entry HTTP endpoints while keeping endpoint logic thin.
using FluentValidation;
using WebApi.Application.ExpenseEntries;
using WebApi.Core.Pagination;

namespace WebApi.Presentation.ExpenseEntries;

/// <summary>
/// HTTP endpoints exposing expense entry management use cases.
/// </summary>
internal static class ExpenseEntryEndpoints
{
    public sealed record ExpenseEntryAddressRequest(string MerchantName, string Street, string PostalCode, string City);

    public sealed record CreateExpenseEntryRequest(
        DateOnly ExpenseDate,
        string Description,
        decimal Amount,
        ExpenseEntryAddressRequest BillingAddress);

    public sealed record UpdateExpenseEntryRequest(
        DateOnly ExpenseDate,
        string Description,
        decimal Amount,
        ExpenseEntryAddressRequest BillingAddress);

    public static IEndpointRouteBuilder MapExpenseEntryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var reportGroup = endpoints.MapGroup("/expense-reports/{expenseReportId:guid}/entries")
            .WithTags("Expense Entries");

        reportGroup.MapGet("", async (
                Guid expenseReportId,
                int? pageNumber,
                IExpenseEntryQueryService queries,
                CancellationToken cancellationToken) =>
            {
                var result = await queries.ListByReportAsync(expenseReportId, pageNumber ?? 1, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("ListExpenseEntries")
            .Produces<PagedResult<ExpenseEntryResult>>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        reportGroup.MapPost("", async (
                Guid expenseReportId,
                CreateExpenseEntryRequest request,
                IExpenseEntryCommandService commands,
                CancellationToken cancellationToken) =>
            {
                var result = await commands.CreateAsync(ToCommand(expenseReportId, request), cancellationToken);
                return Results.Created($"/expense-entries/{result.Id}", result);
            })
            .WithName("CreateExpenseEntry")
            .Produces<ExpenseEntryResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        var entryGroup = endpoints.MapGroup("/expense-entries")
            .WithTags("Expense Entries");

        entryGroup.MapPut("/{id:guid}", async (
                Guid id,
                UpdateExpenseEntryRequest request,
                IExpenseEntryCommandService commands,
                CancellationToken cancellationToken) =>
            {
                var result = await commands.UpdateAsync(ToCommand(id, request), cancellationToken);
                return Results.Ok(result);
            })
            .WithName("UpdateExpenseEntry")
            .Produces<ExpenseEntryResult>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        entryGroup.MapDelete("/{id:guid}", async (
                Guid id,
                IExpenseEntryCommandService commands,
                CancellationToken cancellationToken) =>
            {
                await commands.DeleteAsync(id, cancellationToken);
                return Results.NoContent();
            })
            .WithName("DeleteExpenseEntry")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static CreateExpenseEntryCommand ToCommand(Guid expenseReportId, CreateExpenseEntryRequest request)
    {
        var address = RequireBillingAddress(request.BillingAddress);

        return new CreateExpenseEntryCommand(
            expenseReportId,
            request.ExpenseDate,
            request.Description,
            request.Amount,
            address.MerchantName,
            address.Street,
            address.PostalCode,
            address.City);
    }

    private static UpdateExpenseEntryCommand ToCommand(Guid id, UpdateExpenseEntryRequest request)
    {
        var address = RequireBillingAddress(request.BillingAddress);

        return new UpdateExpenseEntryCommand(
            id,
            request.ExpenseDate,
            request.Description,
            request.Amount,
            address.MerchantName,
            address.Street,
            address.PostalCode,
            address.City);
    }

    private static ExpenseEntryAddressRequest RequireBillingAddress(ExpenseEntryAddressRequest? billingAddress)
    {
        return billingAddress ?? throw new ValidationException("Billing address is required.");
    }
}