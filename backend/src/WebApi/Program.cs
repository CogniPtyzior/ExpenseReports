using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using WebApi.Application;
using WebApi.Infrastructure;
using WebApi.Infrastructure.Persistence;
using WebApi.Presentation;
using WebApi.Presentation.Errors;
using WebApi.Presentation.ExpenseReports;
using WebApi.Presentation.Users;

var builder = WebApplication.CreateBuilder(args);

builder.AddRedisClient(connectionName: "cache");
builder.AddNpgsqlDbContext<AppDbContext>(connectionName: "database",
    configureDbContextOptions: options =>
        options.UseAsyncSeeding(async (context, _, cancellationToken) =>
            await AppDbContext.SeedAsync(context, cancellationToken)));
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
app.MapHealthChecks("/health");

app.Run();

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class Program;