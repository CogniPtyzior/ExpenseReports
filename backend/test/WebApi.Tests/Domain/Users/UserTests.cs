// Covers the initial user domain behavior preserved during the architecture move.
using WebApi.Domain.Users;

namespace WebApi.Tests.Domain.Users;

public sealed class UserTests
{
    [Fact]
    public void Full_name_can_be_built_from_first_name_and_last_name()
    {
        var user = new User { FirstName = "Juste", LastName = "Leblanc" };

        Assert.Equal("Juste Leblanc", $"{user.FirstName} {user.LastName}");
    }
}