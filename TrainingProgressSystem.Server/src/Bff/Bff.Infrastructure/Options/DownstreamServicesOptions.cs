namespace Bff.Infrastructure.Options;

public sealed class DownstreamServicesOptions
{
    public const string SectionName = "DownstreamServices";
    
    public string TrainingGrpcUrl { get; set; } = string.Empty;

    public string AnalyticsGrpcUrl { get; set; } = string.Empty;

    public string NotificationGrpcUrl { get; set; } = string.Empty;
}
