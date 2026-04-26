using Bff.Application.Interfaces.v1;
using Bff.Infrastructure.Extensions;
using Bff.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Bff.Infrastructure.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    private IServiceCollection _services = null!;
    private IConfiguration _configuration = null!;
    private Mock<ILogger> _loggerMock = null!;

    [SetUp]
    public void SetUp()
    {
        _services = new ServiceCollection();
        _loggerMock = new Mock<ILogger>(MockBehavior.Loose);

        var configDict = new Dictionary<string, string?>
        {
            ["DownstreamServices:TrainingGrpcUrl"] = "http://training-service:5000",
            ["DownstreamServices:AnalyticsGrpcUrl"] = "http://analytics-service:5001",
            ["DownstreamServices:NotificationGrpcUrl"] = "http://notification-service:5002"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();
    }

    [Test]
    public void ConfigureInfrastructureServices_RegistersDownstreamServicesOptions()
    {
        // Act
        _services.ConfigureInfrastructureServices(_configuration, _loggerMock.Object);

        var provider = _services.BuildServiceProvider();

        // Assert
        var options = provider.GetRequiredService<IOptions<DownstreamServicesOptions>>().Value;
        Assert.That(options, Is.Not.Null);
        Assert.That(options.TrainingGrpcUrl, Is.EqualTo("http://training-service:5000"));
        Assert.That(options.AnalyticsGrpcUrl, Is.EqualTo("http://analytics-service:5001"));
        Assert.That(options.NotificationGrpcUrl, Is.EqualTo("http://notification-service:5002"));
    }

    [Test]
    public void ConfigureInfrastructureServices_RegistersTrainingSyncClient()
    {
        // Act
        _services.ConfigureInfrastructureServices(_configuration, _loggerMock.Object);

        var provider = _services.BuildServiceProvider();

        // Assert
        var client = provider.GetRequiredService<ITrainingSyncClient>();
        Assert.That(client, Is.Not.Null);
        Assert.That(client, Is.AssignableTo<ITrainingSyncClient>());
    }

    [Test]
    public void ConfigureInfrastructureServices_RegistersAnalyticsSyncClient()
    {
        // Act
        _services.ConfigureInfrastructureServices(_configuration, _loggerMock.Object);

        var provider = _services.BuildServiceProvider();

        // Assert
        var client = provider.GetRequiredService<IAnalyticsSyncClient>();
        Assert.That(client, Is.Not.Null);
        Assert.That(client, Is.AssignableTo<IAnalyticsSyncClient>());
    }

    [Test]
    public void ConfigureInfrastructureServices_RegistersNotificationSyncClient()
    {
        // Act
        _services.ConfigureInfrastructureServices(_configuration, _loggerMock.Object);

        var provider = _services.BuildServiceProvider();

        // Assert
        var client = provider.GetRequiredService<INotificationSyncClient>();
        Assert.That(client, Is.Not.Null);
        Assert.That(client, Is.AssignableTo<INotificationSyncClient>());
    }

    [Test]
    public void ConfigureInfrastructureServices_ClientsAreRegisteredAsScoped()
    {
        // Act
        _services.ConfigureInfrastructureServices(_configuration, _loggerMock.Object);

        // Assert
        var trainingClientDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(ITrainingSyncClient));
        var analyticsClientDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(IAnalyticsSyncClient));
        var notificationClientDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(INotificationSyncClient));

        Assert.That(trainingClientDescriptor?.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
        Assert.That(analyticsClientDescriptor?.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
        Assert.That(notificationClientDescriptor?.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
    }

    [Test]
    public void ConfigureInfrastructureServices_CanResolveSyncClientsFromServiceProvider()
    {
        // Act
        _services.ConfigureInfrastructureServices(_configuration, _loggerMock.Object);

        var provider = _services.BuildServiceProvider();

        // Assert - Verify all three clients can be resolved
        Assert.DoesNotThrow(() =>
        {
            _ = provider.GetRequiredService<ITrainingSyncClient>();
            _ = provider.GetRequiredService<IAnalyticsSyncClient>();
            _ = provider.GetRequiredService<INotificationSyncClient>();
        });
    }

    [Test]
    public void ConfigureInfrastructureServices_WithValidUrls_LoadsOptionsSuccessfully()
    {
        // Act
        _services.ConfigureInfrastructureServices(_configuration, _loggerMock.Object);

        var provider = _services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<DownstreamServicesOptions>>().Value;

        // Assert
        Assert.That(options.TrainingGrpcUrl, Is.Not.Null.And.Not.Empty);
        Assert.That(options.AnalyticsGrpcUrl, Is.Not.Null.And.Not.Empty);
        Assert.That(options.NotificationGrpcUrl, Is.Not.Null.And.Not.Empty);
    }
}
