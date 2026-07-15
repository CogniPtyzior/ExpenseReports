// Centralizes infrastructure adapter registrations.
namespace WebApi.Infrastructure;

/// <summary>
/// Registers infrastructure adapters such as persistence repositories and technical services.
/// </summary>
internal static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        return services;
    }
}