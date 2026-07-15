// Covers presentation-layer dependency injection registration.
using Microsoft.Extensions.DependencyInjection;
using WebApi.Presentation;

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
}