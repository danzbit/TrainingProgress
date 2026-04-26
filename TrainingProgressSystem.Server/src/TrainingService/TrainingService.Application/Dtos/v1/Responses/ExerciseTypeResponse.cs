using Shared.Kernal.Models;

namespace TrainingService.Application.Dtos.v1.Responses;

public record ExerciseTypeResponse(
    Guid Id,
    string Name,
    string Category
);