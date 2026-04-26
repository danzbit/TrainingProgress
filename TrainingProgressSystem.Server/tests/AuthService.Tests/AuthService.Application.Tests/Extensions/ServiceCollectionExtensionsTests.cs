using AuthService.Application.Extensions;
using AuthService.Application.Interfaces.v1;
using AuthService.Application.Services.v1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace AuthService.Application.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void ConfigureApplicationServices_RegistersAuthServiceAsScoped()
    {
        var services = new ServiceCollection();
        var logger = Mock.Of<ILogger>();

        services.ConfigureApplicationServices(logger);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IAuthService));

        Assert.That(descriptor, Is.Not.Null);
        Assert.That(descriptor!.ImplementationType, Is.EqualTo(typeof(AuthService.Application.Services.v1.AuthService)));
        Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
    }
}