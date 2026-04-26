using AiChatService.Domain.Interfaces;
using AiChatService.Infrastructure.Extensions;
using AiChatService.Infrastructure.Llm;
using AiChatService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AiChatService.Infrastructure.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void ConfigureInfrastructureServices_RegistersExpectedServices()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Groq:ApiKey"] = "test-api-key",
                ["Groq:Model"] = "test-model"
            })
            .Build();
        var logger = Mock.Of<ILogger>();

        services.ConfigureInfrastructureServices(configuration, logger);

        var repositoryDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IChatRepository));
        var llmDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ILlmClient));

        Assert.That(repositoryDescriptor, Is.Not.Null);
        Assert.That(repositoryDescriptor!.ImplementationType, Is.EqualTo(typeof(CacheChatRepository)));
        Assert.That(repositoryDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Transient));

        Assert.That(llmDescriptor, Is.Not.Null);
        Assert.That(llmDescriptor!.ImplementationType, Is.EqualTo(typeof(GroqLlmClient)));
        Assert.That(llmDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Transient));

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<GroqOptions>>();
        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var client = httpClientFactory.CreateClient(GroqLlmClient.ClientName);

        Assert.That(options.Value.Model, Is.EqualTo("test-model"));
        Assert.That(client.BaseAddress, Is.EqualTo(new Uri("https://api.groq.com")));
        Assert.That(client.DefaultRequestHeaders.Authorization, Is.Not.Null);
        Assert.That(client.DefaultRequestHeaders.Authorization!.Scheme, Is.EqualTo("Bearer"));
        Assert.That(client.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo("test-api-key"));
    }

    [Test]
    public void ConfigureInfrastructureServices_WhenApiKeyMissing_ThrowsInvalidOperationException()
    {
        var previousApiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY");
        Environment.SetEnvironmentVariable("GROQ_API_KEY", null);

        try
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>())
                .Build();

            var logger = Mock.Of<ILogger>();

            var act = () => services.ConfigureInfrastructureServices(configuration, logger);

            Assert.That(act, Throws.TypeOf<InvalidOperationException>()
                .With.Message.Contains("Groq API key is not configured"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("GROQ_API_KEY", previousApiKey);
        }
    }
}
