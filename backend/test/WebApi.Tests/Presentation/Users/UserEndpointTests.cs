// Covers user HTTP endpoints and presentation error mapping.
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Application.Users;
using WebApi.Core.Exceptions;
using WebApi.Presentation.Errors;
using WebApi.Presentation.Users;

namespace WebApi.Tests.Presentation.Users;

public sealed class UserEndpointTests
{
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public async Task Get_assignable_users_returns_query_results()
    {
        await using var app = await CreateAppAsync(new FakeUserQueryService(), new FakeUserCommandService());
        var client = app.GetTestClient();

        var response = await client.GetAsync("/users/assignable");

        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Juste Leblanc", json.RootElement[0].GetProperty("fullName").GetString());
    }

    [Fact]
    public async Task Post_user_returns_created_user()
    {
        await using var app = await CreateAppAsync(new FakeUserQueryService(), new FakeUserCommandService());
        var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync("/users", Request());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/users/{UserId}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Put_user_returns_updated_user()
    {
        await using var app = await CreateAppAsync(new FakeUserQueryService(), new FakeUserCommandService());
        var client = app.GetTestClient();

        var response = await client.PutAsJsonAsync($"/users/{UserId}", Request());

        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Juste Leblanc", json.RootElement.GetProperty("fullName").GetString());
    }

    [Fact]
    public async Task Missing_user_is_mapped_to_not_found()
    {
        await using var app = await CreateAppAsync(new MissingUserQueryService(), new FakeUserCommandService());
        var client = app.GetTestClient();

        var response = await client.GetAsync($"/users/{UserId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("user.not_found", json.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Validation_error_is_mapped_to_bad_request()
    {
        await using var app = await CreateAppAsync(new FakeUserQueryService(), new InvalidUserCommandService());
        var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync("/users", Request());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("validation.failed", json.RootElement.GetProperty("code").GetString());
    }

    private static async Task<WebApplication> CreateAppAsync(IUserQueryService queries, IUserCommandService commands)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddLogging();
        builder.Services.AddSingleton(queries);
        builder.Services.AddSingleton(commands);

        var app = builder.Build();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.MapUserEndpoints();
        await app.StartAsync();
        return app;
    }

    private static object Request()
    {
        return new
        {
            FirstName = "Juste",
            LastName = "Leblanc",
            Street = "1 Rue des Notes",
            PostalCode = "75001",
            City = "Paris",
            MonthlyExpenseQuota = 5
        };
    }

    private static UserResult Result(bool isActive = true)
    {
        return new UserResult(UserId, "Juste", "Leblanc", "Juste Leblanc", "1 Rue des Notes", "75001", "Paris", 5, isActive, isActive);
    }

    private class FakeUserQueryService : IUserQueryService
    {
        public virtual Task<IReadOnlyCollection<UserResult>> ListAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<UserResult>>([Result()]);
        }

        public virtual Task<IReadOnlyCollection<UserResult>> ListAssignableAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<UserResult>>([Result()]);
        }

        public virtual Task<UserResult> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result());
        }
    }

    private sealed class MissingUserQueryService : FakeUserQueryService
    {
        public override Task<UserResult> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotFoundException("user.not_found", "User was not found.");
        }
    }

    private class FakeUserCommandService : IUserCommandService
    {
        public virtual Task<UserResult> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result());
        }

        public virtual Task<UserResult> UpdateAsync(UpdateUserCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result());
        }

        public virtual Task<UserResult> ActivateAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result());
        }

        public virtual Task<UserResult> DeactivateAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result(isActive: false));
        }

        public virtual Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class InvalidUserCommandService : FakeUserCommandService
    {
        public override Task<UserResult> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken)
        {
            throw new ValidationException([new ValidationFailure("FirstName", "First name is required.")]);
        }
    }
}