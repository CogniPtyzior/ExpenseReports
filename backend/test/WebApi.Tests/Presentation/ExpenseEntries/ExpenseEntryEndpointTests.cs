// Covers expense entry HTTP endpoints and presentation error mapping.
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Application.ExpenseEntries;
using WebApi.Core.Exceptions;
using WebApi.Core.Pagination;
using WebApi.Presentation.Errors;
using WebApi.Presentation.ExpenseEntries;

namespace WebApi.Tests.Presentation.ExpenseEntries;

public sealed class ExpenseEntryEndpointTests
{
    private static readonly Guid ReportId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid EntryId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Fact]
    public async Task Get_entries_returns_paged_entries()
    {
        await using var app = await CreateAppAsync(new FakeExpenseEntryCommandService(), new FakeExpenseEntryQueryService());
        var client = app.GetTestClient();

        var response = await client.GetAsync($"/expense-reports/{ReportId}/entries?pageNumber=2");

        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(2, json.RootElement.GetProperty("pageNumber").GetInt32());
        Assert.Equal(5, json.RootElement.GetProperty("pageSize").GetInt32());
        Assert.Equal(6, json.RootElement.GetProperty("totalCount").GetInt32());
        Assert.Equal(EntryId, json.RootElement.GetProperty("items")[0].GetProperty("id").GetGuid());
    }
    [Fact]
    public async Task Post_entry_returns_created_entry()
    {
        await using var app = await CreateAppAsync(new FakeExpenseEntryCommandService());
        var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync($"/expense-reports/{ReportId}/entries", Request());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/expense-entries/{EntryId}", response.Headers.Location?.ToString());
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("2025-10-15", json.RootElement.GetProperty("expenseDate").GetString());
    }

    [Fact]
    public async Task Put_entry_returns_updated_entry()
    {
        await using var app = await CreateAppAsync(new FakeExpenseEntryCommandService());
        var client = app.GetTestClient();

        var response = await client.PutAsJsonAsync($"/expense-entries/{EntryId}", Request());

        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Restaurant", json.RootElement.GetProperty("description").GetString());
    }

    [Fact]
    public async Task Delete_entry_returns_no_content()
    {
        await using var app = await CreateAppAsync(new FakeExpenseEntryCommandService());
        var client = app.GetTestClient();

        var response = await client.DeleteAsync($"/expense-entries/{EntryId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Missing_entry_is_mapped_to_not_found()
    {
        await using var app = await CreateAppAsync(new MissingExpenseEntryCommandService());
        var client = app.GetTestClient();

        var response = await client.PutAsJsonAsync($"/expense-entries/{EntryId}", Request());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("expense_entry.not_found", json.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Validation_error_is_mapped_to_bad_request()
    {
        await using var app = await CreateAppAsync(new InvalidExpenseEntryCommandService());
        var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync($"/expense-reports/{ReportId}/entries", Request());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("validation.failed", json.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Quota_conflict_is_mapped_to_conflict()
    {
        await using var app = await CreateAppAsync(new QuotaReachedExpenseEntryCommandService());
        var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync($"/expense-reports/{ReportId}/entries", Request());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("expense_entry.monthly_quota_reached", json.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Missing_billing_address_is_mapped_to_bad_request()
    {
        await using var app = await CreateAppAsync(new FakeExpenseEntryCommandService());
        var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync($"/expense-reports/{ReportId}/entries", new
        {
            ExpenseDate = "2025-10-15",
            Description = "Restaurant",
            Amount = 42.50m
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("validation.failed", json.RootElement.GetProperty("code").GetString());
    }
    private static async Task<WebApplication> CreateAppAsync(
        IExpenseEntryCommandService commands,
        IExpenseEntryQueryService? queries = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddLogging();
        builder.Services.AddSingleton(commands);
        builder.Services.AddSingleton(queries ?? new FakeExpenseEntryQueryService());

        var app = builder.Build();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.MapExpenseEntryEndpoints();
        await app.StartAsync();
        return app;
    }

    private static object Request()
    {
        return new
        {
            ExpenseDate = "2025-10-15",
            Description = "Restaurant",
            Amount = 42.50m,
            BillingAddress = new
            {
                MerchantName = "Cafe Central",
                Street = "1 Rue des Notes",
                PostalCode = "75001",
                City = "Paris"
            }
        };
    }

    private static ExpenseEntryResult Result()
    {
        return new ExpenseEntryResult(
            EntryId,
            ReportId,
            2025,
            10,
            new DateOnly(2025, 10, 15),
            "Restaurant",
            42.50m,
            "EUR",
            "Cafe Central",
            "1 Rue des Notes",
            "75001",
            "Paris",
            DateTime.UtcNow,
            null);
    }

    private sealed class FakeExpenseEntryQueryService : IExpenseEntryQueryService
    {
        public Task<PagedResult<ExpenseEntryResult>> ListByReportAsync(
            Guid expenseReportId,
            int pageNumber,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new PagedResult<ExpenseEntryResult>([Result()], pageNumber, 5, 6));
        }
    }

    private class FakeExpenseEntryCommandService : IExpenseEntryCommandService
    {
        public virtual Task<ExpenseEntryResult> CreateAsync(
            CreateExpenseEntryCommand command,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Result());
        }

        public virtual Task<ExpenseEntryResult> UpdateAsync(
            UpdateExpenseEntryCommand command,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Result());
        }

        public virtual Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class MissingExpenseEntryCommandService : FakeExpenseEntryCommandService
    {
        public override Task<ExpenseEntryResult> UpdateAsync(
            UpdateExpenseEntryCommand command,
            CancellationToken cancellationToken)
        {
            throw new NotFoundException("expense_entry.not_found", "Expense entry was not found.");
        }
    }

    private sealed class QuotaReachedExpenseEntryCommandService : FakeExpenseEntryCommandService
    {
        public override Task<ExpenseEntryResult> CreateAsync(
            CreateExpenseEntryCommand command,
            CancellationToken cancellationToken)
        {
            throw new ConflictException("expense_entry.monthly_quota_reached", "Monthly expense quota has been reached.");
        }
    }

    private sealed class InvalidExpenseEntryCommandService : FakeExpenseEntryCommandService
    {
        public override Task<ExpenseEntryResult> CreateAsync(
            CreateExpenseEntryCommand command,
            CancellationToken cancellationToken)
        {
            throw new ValidationException([new ValidationFailure("Description", "Description is required.")]);
        }
    }
}
