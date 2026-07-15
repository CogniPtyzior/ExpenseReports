// Covers infrastructure-layer dependency injection registration.
using Microsoft.Extensions.DependencyInjection;
using WebApi.Infrastructure;

namespace WebApi.Tests.Infrastructure;

public sealed class InfrastructureDependencyInjectionTests
{
    [Fact]
    public void Infrastructure_services_can_be_registered()
    {
        var services = new ServiceCollection();

        var returned = services.AddInfrastructure();

        Assert.Same(services, returned);
    }
}