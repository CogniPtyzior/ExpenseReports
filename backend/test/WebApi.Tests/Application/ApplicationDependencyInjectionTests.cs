// Covers application-layer dependency injection registration.
using Microsoft.Extensions.DependencyInjection;
using WebApi.Application;
using WebApi.Application.Core.Clock;
using WebApi.Application.Core.Observability;
using WebApi.Application.Core.Persistence;
using WebApi.Application.Users;
using WebApi.Domain.Users;

namespace WebApi.Tests.Application;

public sealed class ApplicationDependencyInjectionTests
{
    [Fact]
    public void Application_services_can_be_registered_and_resolved()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddSingleton<IClock, FixedClock>();
        services.AddSingleton<IApplicationLogger, NullApplicationLogger>();
        services.AddScoped<IUnitOfWork, NoopUnitOfWork>();
        services.AddScoped<IUserRepository, EmptyUserRepository>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IUserCommandService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IUserQueryService>());
    }

    private sealed class FixedClock : IClock
    {
        public DateTime UtcNow => new(2025, 10, 15, 8, 30, 0, DateTimeKind.Utc);
    }

    private sealed class NullApplicationLogger : IApplicationLogger
    {
        public void Information(string message, params object[] args)
        {
        }
    }

    private sealed class NoopUnitOfWork : IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class EmptyUserRepository : IUserRepository
    {
        public Task<IReadOnlyCollection<User>> ListAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<User>>([]);
        }

        public Task<IReadOnlyCollection<User>> ListAssignableAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<User>>([]);
        }

        public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult<User?>(null);
        }

        public Task AddAsync(User user, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}