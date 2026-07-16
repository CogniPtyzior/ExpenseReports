// Covers expense report HTTP endpoints and presentation error mapping.
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Application.ExpenseReports;
using WebApi.Core.Exceptions;
using WebApi.Presentation.Errors;
using WebApi.Presentation.ExpenseReports;

namespace WebApi.Tests.Presentation.ExpenseReports;

public sealed class ExpenseReportEndpointTests
{
    private static readonly Guid ReportId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task Get_reports_returns_query_results()
    {
        await using var app = await CreateAppAsync(new FakeExpenseReportQueryService(), new FakeExpenseReportCommandService());
        var client = app.GetTestClient();

        var response = await client.GetAsync("/expense-reports");

        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Juste Leblanc - Octobre 2025", json.RootElement[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task Post_report_returns_created_report()
    {
        await using var app = await CreateAppAsync(new FakeExpenseReportQueryService(), new FakeExpenseReportCommandService());
        var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync("/expense-reports", Request());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/expense-reports/{ReportId}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Missing_report_is_mapped_to_not_found()
    {
        await using var app = await CreateAppAsync(new MissingExpenseReportQueryService(), new FakeExpenseReportCommandService());
        var client = app.GetTestClient();

        var response = await client.GetAsync($"/expense-reports/{ReportId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("expense_report.not_found", json.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Existing_report_conflict_is_mapped_to_conflict()
    {
        await using var app = await CreateAppAsync(new FakeExpenseReportQueryService(), new ConflictExpenseReportCommandService());
        var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync("/expense-reports", Request());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("expense_report.already_exists", json.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Validation_error_is_mapped_to_bad_request()
    {
        await using var app = await CreateAppAsync(new FakeExpenseReportQueryService(), new InvalidExpenseReportCommandService());
        var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync("/expense-reports", Request());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("validation.failed", json.RootElement.GetProperty("code").GetString());
    }

    private static async Task<WebApplication> CreateAppAsync(
        IExpenseReportQueryService queries,
        IExpenseReportCommandService commands)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddLogging();
        builder.Services.AddSingleton(queries);
        builder.Services.AddSingleton(commands);

        var app = builder.Build();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.MapExpenseReportEndpoints();
        await app.StartAsync();
        return app;
    }

    private static object Request()
    {
        return new { UserId, Year = 2025, Month = 10 };
    }

    private static ExpenseReportResult Result()
    {
        return new ExpenseReportResult(ReportId, UserId, "Juste Leblanc", 2025, 10, "Juste Leblanc - Octobre 2025", DateTime.UtcNow);
    }

    private class FakeExpenseReportQueryService : IExpenseReportQueryService
    {
        public virtual Task<IReadOnlyCollection<ExpenseReportResult>> ListAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<ExpenseReportResult>>([Result()]);
        }

        public virtual Task<ExpenseReportResult> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result());
        }
    }

    private sealed class MissingExpenseReportQueryService : FakeExpenseReportQueryService
    {
        public override Task<ExpenseReportResult> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotFoundException("expense_report.not_found", "Expense report was not found.");
        }
    }

    private class FakeExpenseReportCommandService : IExpenseReportCommandService
    {
        public virtual Task<ExpenseReportResult> CreateAsync(CreateExpenseReportCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result());
        }
    }

    private sealed class ConflictExpenseReportCommandService : FakeExpenseReportCommandService
    {
        public override Task<ExpenseReportResult> CreateAsync(CreateExpenseReportCommand command, CancellationToken cancellationToken)
        {
            throw new ConflictException("expense_report.already_exists", "An expense report already exists.");
        }
    }

    private sealed class InvalidExpenseReportCommandService : FakeExpenseReportCommandService
    {
        public override Task<ExpenseReportResult> CreateAsync(CreateExpenseReportCommand command, CancellationToken cancellationToken)
        {
            throw new ValidationException([new ValidationFailure("Month", "Month must be valid.")]);
        }
    }
}