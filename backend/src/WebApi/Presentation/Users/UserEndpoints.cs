// Exposes user-related HTTP endpoints while keeping endpoint logic thin.
using WebApi.Domain.Users;
using WebApi.Infrastructure.Persistence;

namespace WebApi.Presentation.Users;

/// <summary>
/// HTTP endpoints exposing user read operations.
/// </summary>
internal static class UserEndpoints
{
    public sealed record UserInfo(Guid Id, string Name);

    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/users", (AppDbContext db) => Results.Ok(
                db.Set<User>().Select(u => new UserInfo(u.Id, $"{u.FirstName} {u.LastName}"))))
            .WithName("GetUsers")
            .WithTags("Users")
            .Produces<IEnumerable<UserInfo>>();

        return endpoints;
    }
}