// Covers infrastructure-layer dependency injection registration.
using Microsoft.Extensions.DependencyInjection;
using WebApi.Infrastructure;
using WebApi.Application.Core.Clock;
using WebApi.Application.Core.Observability;
using WebApi.Application.Core.Persistence;
using WebApi.Infrastructure.Clock;
using WebApi.Application.ExpenseEntries;
using WebApi.Application.ExpenseReports;
using WebApi.Application.Users;
using WebApi.Infrastructure.Observability;
using WebApi.Infrastructure.Persistence;

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

    [Fact]
    public void Infrastructure_registers_expected_services()
    {
        var services = new ServiceCollection();

        services.AddInfrastructure();

        AssertRegistration<IClock, SystemClock>(
            services,
            ServiceLifetime.Singleton);

        AssertRegistration<IUnitOfWork, EfUnitOfWork>(
            services,
            ServiceLifetime.Scoped);

        AssertRegistration<IUserRepository, UserRepository>(
            services,
            ServiceLifetime.Scoped);

        AssertRegistration<IExpenseReportRepository, ExpenseReportRepository>(
            services,
            ServiceLifetime.Scoped);

        AssertRegistration<IExpenseEntryRepository, ExpenseEntryRepository>(
            services,
            ServiceLifetime.Scoped);

        AssertRegistration<
            IApplicationLogger,
            MicrosoftApplicationLogger<IApplicationLogger>>(
            services,
            ServiceLifetime.Scoped);
    }

    private static void AssertRegistration<TService, TImplementation>(
        IServiceCollection services,
        ServiceLifetime expectedLifetime)
    {
        var descriptor = Assert.Single(
            services,
            service => service.ServiceType == typeof(TService));

        Assert.Equal(
            typeof(TImplementation),
            descriptor.ImplementationType);

        Assert.Equal(
            expectedLifetime,
            descriptor.Lifetime);
    }
}
