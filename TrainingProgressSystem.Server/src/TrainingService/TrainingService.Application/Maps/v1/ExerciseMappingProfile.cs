using AutoMapper;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Domain.Entities;

namespace TrainingService.Application.Maps.v1;

public class ExerciseMappingProfile : Profile
{
    public ExerciseMappingProfile()
    {
        CreateMap<Exercise, ExerciseResponse>();
        CreateMap<ExerciseType, ExerciseTypeResponse>();
        CreateMap<ExerciseRequest, Exercise>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ExerciseId));
    }
}