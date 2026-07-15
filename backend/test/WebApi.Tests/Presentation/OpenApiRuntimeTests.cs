// Covers runtime endpoint construction when OpenAPI metadata and Swagger services are enabled.
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Application.Users;
using WebApi.Presentation;
using WebApi.Presentation.Users;

namespace WebApi.Tests.Presentation;

public sealed class OpenApiRuntimeTests
{
    [Fact]
    public async Task OpenApi_services_do_not_break_endpoint_routing()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddOpenApi().AddPresentation();
        builder.Services.AddSingleton<IUserQueryService, EmptyUserQueryService>();
        builder.Services.AddSingleton<IUserCommandService, NoopUserCommandService>();

        await using var app = builder.Build();
        app.MapOpenApi();
        app.UseSwagger();
        app.MapUserEndpoints();
        await app.StartAsync();

        var response = await app.GetTestClient().GetAsync("/users");

        response.EnsureSuccessStatusCode();
    }

    private sealed class EmptyUserQueryService : IUserQueryService
    {
        public Task<IReadOnlyCollection<UserResult>> ListAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<UserResult>>([]);
        }

        public Task<IReadOnlyCollection<UserResult>> ListAssignableAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<UserResult>>([]);
        }

        public Task<UserResult> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("This test does not query a single user.");
        }
    }

    private sealed class NoopUserCommandService : IUserCommandService
    {
        public Task<UserResult> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("This test does not create users.");
        }

        public Task<UserResult> UpdateAsync(UpdateUserCommand command, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("This test does not update users.");
        }

        public Task<UserResult> ActivateAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("This test does not activate users.");
        }

        public Task<UserResult> DeactivateAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("This test does not deactivate users.");
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("This test does not delete users.");
        }
    }
}