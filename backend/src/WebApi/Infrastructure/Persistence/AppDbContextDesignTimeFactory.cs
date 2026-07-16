// Provides a design-time DbContext factory for EF Core migration tooling.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebApi.Infrastructure.Persistence;

/// <summary>
/// Creates AppDbContext for EF Core CLI commands without requiring the Aspire host to run.
/// </summary>
internal sealed class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string ConnectionStringKey = "ConnectionStrings__database";

    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = ResolveConnectionString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    private static string ResolveConnectionString()
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringKey);
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        connectionString = TryReadDotEnvValue(ConnectionStringKey);
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        throw new InvalidOperationException(
            $"Missing '{ConnectionStringKey}'. Set it as an environment variable or in a local .env file.");
    }

    private static string? TryReadDotEnvValue(string key)
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, ".env");
            if (File.Exists(candidate) && TryReadDotEnvValue(candidate, key, out var value))
            {
                return value;
            }

            current = current.Parent;
        }

        return null;
    }

    private static bool TryReadDotEnvValue(string path, string key, out string? value)
    {
        foreach (var line in File.ReadLines(path))
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.Length == 0 || trimmedLine.StartsWith('#'))
            {
                continue;
            }

            var separatorIndex = trimmedLine.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var name = trimmedLine[..separatorIndex].Trim();
            if (!string.Equals(name, key, StringComparison.Ordinal))
            {
                continue;
            }

            value = trimmedLine[(separatorIndex + 1)..].Trim().Trim('"');
            return true;
        }

        value = null;
        return false;
    }
}
