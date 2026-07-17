using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using WebApi.Application;
using WebApi.Application.Core.Configuration;
using WebApi.Infrastructure;
using WebApi.Infrastructure.Persistence;
using WebApi.Infrastructure.Persistence.Seeding;
using WebApi.Presentation;
using WebApi.Presentation.Errors;
using WebApi.Presentation.ExpenseEntries;
using WebApi.Presentation.ExpenseReports;
using WebApi.Presentation.Users;

var builder = WebApplication.CreateBuilder(args);

builder.AddRedisClient(connectionName: "cache");
builder.AddNpgsqlDbContext<AppDbContext>(connectionName: "database",
    configureDbContextOptions: options =>
    {
        // EF seeding runs during MigrateAsync, which is already limited to Development below.
        // The explicit guard keeps demo data out of non-development configurations without ambiguity.
        if (builder.Environment.IsDevelopment())
        {
            options.UseAsyncSeeding(async (context, _, cancellationToken) =>
                await DevelopmentDataSeeder.SeedAsync(context, cancellationToken));
        }
    });
builder.Services.AddExpenseRulesOptions(builder.Configuration);

builder.Services
    .AddOpenApi()
    .AddApplication()
    .AddInfrastructure()
    .AddPresentation();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    try
    {
        using var sp = app.Services.CreateScope();
        await sp.ServiceProvider.GetRequiredService<AppDbContext>()
            .Database.MigrateAsync();
    }
    catch (Exception e)
    {
        app.Services.GetRequiredService<ILogger<Program>>()
            .LogError(e, "An error occurred while migrating the database.");
        return;
    }

    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapUserEndpoints();
app.MapExpenseReportEndpoints();
app.MapExpenseEntryEndpoints();
app.MapHealthChecks("/health");

app.Run();

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class Program;
