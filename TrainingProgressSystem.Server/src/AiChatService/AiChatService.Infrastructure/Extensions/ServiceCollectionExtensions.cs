using AiChatService.Domain.Interfaces;
using AiChatService.Infrastructure.Llm;
using AiChatService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AiChatService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger logger)
    {
        services.AddTransient<IChatRepository, CacheChatRepository>();
        logger.LogInformation("Registered chat repository (cache-backed)");

        services.Configure<GroqOptions>(configuration.GetSection("Groq"));

        var groqApiKey = configuration["Groq:ApiKey"]
            ?? Environment.GetEnvironmentVariable("GROQ_API_KEY")
            ?? throw new InvalidOperationException(
                "Groq API key is not configured. Set Groq:ApiKey or GROQ_API_KEY.");

        // Register ILlmClient with IHttpClientFactory under a named client.
        // The typed registration forwards the factory to GroqLlmClient via constructor.
        services.AddTransient<ILlmClient, GroqLlmClient>();

        services.AddHttpClient(GroqLlmClient.ClientName, client =>
            {
                client.BaseAddress = new Uri("https://api.groq.com");
                client.Timeout = TimeSpan.FromSeconds(90); // SSE streams can take time
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {groqApiKey}");
            })
            .AddStandardResilienceHandler(options =>
            {
                // Total request timeout (outer bound covering all retries)
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(120);

                // Per-attempt timeout (each individual try)
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);

                // Retry: 3 attempts with exponential back-off (1s, 2s, 4s)
                // Only retries transient HTTP errors (5xx, 408, 429) and network failures
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromSeconds(1);
                options.Retry.UseJitter = true;

                // Circuit breaker: sampling duration must be > 2x attempt timeout (30s × 2 = 60s minimum)
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);
                options.CircuitBreaker.FailureRatio = 0.5;
                options.CircuitBreaker.MinimumThroughput = 3;
                options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
            });

        logger.LogInformation("Registered Groq LLM client with resilience pipeline (retry + circuit breaker)");

        logger.LogInformation("Configured infrastructure services");
    }
}
