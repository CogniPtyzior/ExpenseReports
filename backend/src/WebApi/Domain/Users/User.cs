// Defines the initial user aggregate used by the current API and persistence model.
namespace WebApi.Domain.Users;

/// <summary>
/// User aggregate currently backed by the initial seed and read endpoint.
/// </summary>
internal sealed class User
{
    public Guid Id { get; set; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
}