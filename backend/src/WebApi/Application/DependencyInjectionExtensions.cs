// Centralizes application-layer service registrations.
using FluentValidation;
using WebApi.Application.Users;

namespace WebApi.Application;

/// <summary>
/// Registers application services and validators.
/// </summary>
internal static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>(includeInternalTypes: true);
        services.AddScoped<IUserCommandService, UserCommandService>();
        services.AddScoped<IUserQueryService, UserQueryService>();

        return services;
    }
}