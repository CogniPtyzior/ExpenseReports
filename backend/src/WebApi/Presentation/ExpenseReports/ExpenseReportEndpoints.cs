// Exposes expense report HTTP endpoints while keeping endpoint logic thin.
using WebApi.Application.ExpenseReports;

namespace WebApi.Presentation.ExpenseReports;

/// <summary>
/// HTTP endpoints exposing expense report management use cases.
/// </summary>
internal static class ExpenseReportEndpoints
{
    public sealed record CreateExpenseReportRequest(Guid UserId, int Year, int Month);

    public static IEndpointRouteBuilder MapExpenseReportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/expense-reports")
            .WithTags("Expense Reports");

        group.MapGet("", async (IExpenseReportQueryService queries, CancellationToken cancellationToken) =>
                Results.Ok(await queries.ListAsync(cancellationToken)))
            .WithName("ListExpenseReports")
            .Produces<IReadOnlyCollection<ExpenseReportResult>>();

        group.MapGet("/{id:guid}", async (Guid id, IExpenseReportQueryService queries, CancellationToken cancellationToken) =>
                Results.Ok(await queries.GetAsync(id, cancellationToken)))
            .WithName("GetExpenseReport")
            .Produces<ExpenseReportResult>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("", async (
                CreateExpenseReportRequest request,
                IExpenseReportCommandService commands,
                CancellationToken cancellationToken) =>
            {
                var result = await commands.CreateAsync(ToCommand(request), cancellationToken);
                return Results.Created($"/expense-reports/{result.Id}", result);
            })
            .WithName("CreateExpenseReport")
            .Produces<ExpenseReportResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static CreateExpenseReportCommand ToCommand(CreateExpenseReportRequest request)
    {
        return new CreateExpenseReportCommand(request.UserId, request.Year, request.Month);
    }
}