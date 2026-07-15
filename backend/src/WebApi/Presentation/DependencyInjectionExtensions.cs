// Centralizes HTTP presentation registrations.
namespace WebApi.Presentation;

/// <summary>
/// Registers presentation concerns for the HTTP API.
/// </summary>
internal static class DependencyInjectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}