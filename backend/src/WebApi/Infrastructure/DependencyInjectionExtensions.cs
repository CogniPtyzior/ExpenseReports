// Centralizes infrastructure adapter registrations.
using WebApi.Application.Core.Clock;
using WebApi.Application.Core.Observability;
using WebApi.Application.Core.Persistence;
using WebApi.Application.ExpenseEntries;
using WebApi.Application.ExpenseReports;
using WebApi.Application.Users;
using WebApi.Infrastructure.Clock;
using WebApi.Infrastructure.Observability;
using WebApi.Infrastructure.Persistence;

namespace WebApi.Infrastructure;

/// <summary>
/// Registers infrastructure adapters such as persistence repositories and technical services.
/// </summary>
internal static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IExpenseReportRepository, ExpenseReportRepository>();
        services.AddScoped<IExpenseEntryRepository, ExpenseEntryRepository>();
        services.AddScoped<IApplicationLogger, MicrosoftApplicationLogger<UserCommandService>>();

        return services;
    }
}