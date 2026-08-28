// Covers presentation-layer dependency injection registration.
using Microsoft.Extensions.DependencyInjection;
using WebApi.Presentation;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.Tests.Presentation;

public sealed class PresentationDependencyInjectionTests
{
    [Fact]
    public void Presentation_services_can_be_registered()
    {
        var services = new ServiceCollection();

        var returned = services.AddPresentation();

        Assert.Same(services, returned);
    }

    [Fact]
    public void Presentation_registers_expected_services()
    {
        var services = new ServiceCollection();

        services.AddPresentation();

        AssertServiceRegistered<ISwaggerProvider>(services);
    }

    private static void AssertServiceRegistered<TService>(
        IServiceCollection services)
    {
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(TService));
    }
}
