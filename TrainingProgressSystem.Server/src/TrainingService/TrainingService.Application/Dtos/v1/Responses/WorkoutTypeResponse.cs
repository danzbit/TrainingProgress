using Shared.Kernal.Models;

namespace TrainingService.Application.Dtos.v1.Responses;

public record WorkoutTypeResponse(
    Guid Id,
    string Name,
    string? Description
);