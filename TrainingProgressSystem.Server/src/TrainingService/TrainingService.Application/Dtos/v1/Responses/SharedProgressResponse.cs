namespace TrainingService.Application.Dtos.v1.Responses;

public sealed record SharedProgressResponse(
    string Title,
    string? Description,
    DateTime CreatedAt,
    DateTime? Expiration
);
