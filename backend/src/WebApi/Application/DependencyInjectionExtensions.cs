// Centralizes application-layer service registrations.
namespace WebApi.Application;

/// <summary>
/// Registers application services and validators.
/// </summary>
internal static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}