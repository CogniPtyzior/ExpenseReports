// Covers application-layer dependency injection registration.
using Microsoft.Extensions.DependencyInjection;
using WebApi.Application;

namespace WebApi.Tests.Application;

public sealed class ApplicationDependencyInjectionTests
{
    [Fact]
    public void Application_services_can_be_registered()
    {
        var services = new ServiceCollection();

        var returned = services.AddApplication();

        Assert.Same(services, returned);
    }
}