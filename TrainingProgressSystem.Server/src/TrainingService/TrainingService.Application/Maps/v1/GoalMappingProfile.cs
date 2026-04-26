using AutoMapper;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Domain.Entities;
using TrainingService.Domain.Enums;

namespace TrainingService.Application.Maps.v1;

public sealed class GoalMappingProfile : Profile
{
    public GoalMappingProfile()
    {
        CreateMap<Goal, GoalsResponse>()
            .ForCtorParam(nameof(GoalsResponse.CurrentValue), opt =>
                opt.MapFrom(src => src.Progress != null
                    ? src.Progress.CurrentValue
                    : (src.Status == GoalStatus.Completed ? src.TargetValue : 0)))
            .ForCtorParam(nameof(GoalsResponse.ProgressPercentage), opt =>
                opt.MapFrom(src => src.Progress != null
                    ? src.Progress.Percentage
                    : (src.Status == GoalStatus.Completed ? 100d : 0d)))
            .ForCtorParam(nameof(GoalsResponse.IsCompleted), opt =>
                opt.MapFrom(src => src.Progress != null
                    ? src.Progress.IsCompleted
                    : src.Status == GoalStatus.Completed))
            .ForCtorParam(nameof(GoalsResponse.LastCalculatedAt), opt =>
                opt.MapFrom(src => src.Progress != null
                    ? src.Progress.LastCalculatedAt
                    : (DateTime?)null));

        CreateMap<UpdateGoalRequest, Goal>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.GoalId));
    }
}
