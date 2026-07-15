// Exposes user-related HTTP endpoints while keeping endpoint logic thin.
using WebApi.Application.Users;

namespace WebApi.Presentation.Users;

/// <summary>
/// HTTP endpoints exposing user management use cases.
/// </summary>
internal static class UserEndpoints
{
    public sealed record CreateUserRequest(
        string FirstName,
        string LastName,
        string Street,
        string PostalCode,
        string City,
        int MonthlyExpenseQuota);

    public sealed record UpdateUserRequest(
        string FirstName,
        string LastName,
        string Street,
        string PostalCode,
        string City,
        int MonthlyExpenseQuota);

    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/users")
            .WithTags("Users");

        group.MapGet("", async (IUserQueryService queries, CancellationToken cancellationToken) =>
                Results.Ok(await queries.ListAsync(cancellationToken)))
            .WithName("ListUsers")
            .Produces<IReadOnlyCollection<UserResult>>();

        group.MapGet("/assignable", async (IUserQueryService queries, CancellationToken cancellationToken) =>
                Results.Ok(await queries.ListAssignableAsync(cancellationToken)))
            .WithName("ListAssignableUsers")
            .Produces<IReadOnlyCollection<UserResult>>();

        group.MapGet("/{id:guid}", async (Guid id, IUserQueryService queries, CancellationToken cancellationToken) =>
                Results.Ok(await queries.GetAsync(id, cancellationToken)))
            .WithName("GetUser")
            .Produces<UserResult>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("", async (
                CreateUserRequest request,
                IUserCommandService commands,
                CancellationToken cancellationToken) =>
            {
                var result = await commands.CreateAsync(ToCommand(request), cancellationToken);
                return Results.Created($"/users/{result.Id}", result);
            })
            .WithName("CreateUser")
            .Produces<UserResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (
                Guid id,
                UpdateUserRequest request,
                IUserCommandService commands,
                CancellationToken cancellationToken) =>
            {
                var result = await commands.UpdateAsync(ToCommand(id, request), cancellationToken);
                return Results.Ok(result);
            })
            .WithName("UpdateUser")
            .Produces<UserResult>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/activate", async (
                Guid id,
                IUserCommandService commands,
                CancellationToken cancellationToken) =>
            {
                var result = await commands.ActivateAsync(id, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("ActivateUser")
            .Produces<UserResult>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/deactivate", async (
                Guid id,
                IUserCommandService commands,
                CancellationToken cancellationToken) =>
            {
                var result = await commands.DeactivateAsync(id, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("DeactivateUser")
            .Produces<UserResult>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IUserCommandService commands, CancellationToken cancellationToken) =>
            {
                await commands.DeleteAsync(id, cancellationToken);
                return Results.NoContent();
            })
            .WithName("DeleteUser")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static CreateUserCommand ToCommand(CreateUserRequest request)
    {
        return new CreateUserCommand(
            request.FirstName,
            request.LastName,
            request.Street,
            request.PostalCode,
            request.City,
            request.MonthlyExpenseQuota);
    }

    private static UpdateUserCommand ToCommand(Guid id, UpdateUserRequest request)
    {
        return new UpdateUserCommand(
            id,
            request.FirstName,
            request.LastName,
            request.Street,
            request.PostalCode,
            request.City,
            request.MonthlyExpenseQuota);
    }
}