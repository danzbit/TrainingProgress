using AutoMapper;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Domain.Entities;

namespace TrainingService.Application.Maps.v1;

public class WorkoutMappingProfile : Profile
{
    public WorkoutMappingProfile()
    {
        CreateMap<Workout, WorkoutsResponse>();
        CreateMap<WorkoutType, WorkoutTypeResponse>();
        CreateMap<UpdateWorkoutRequest, Workout>()
         .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.WorkoutId));
    }
}
