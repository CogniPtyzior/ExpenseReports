// Covers design-time DbContext configuration used by EF Core CLI commands.
using Microsoft.EntityFrameworkCore;
using WebApi.Infrastructure.Persistence;

namespace WebApi.Tests.Infrastructure.Persistence;

public sealed class AppDbContextDesignTimeFactoryTests
{
    [Fact]
    public void Factory_uses_connection_string_from_environment()
    {
        const string key = "ConnectionStrings__database";
        const string connectionString = "Host=localhost;Database=test;Username=test;Password=test";
        var previous = Environment.GetEnvironmentVariable(key);

        try
        {
            Environment.SetEnvironmentVariable(key, connectionString);

            using var db = new AppDbContextDesignTimeFactory().CreateDbContext([]);

            Assert.Equal(connectionString, db.Database.GetConnectionString());
        }
        finally
        {
            Environment.SetEnvironmentVariable(key, previous);
        }
    }
}
