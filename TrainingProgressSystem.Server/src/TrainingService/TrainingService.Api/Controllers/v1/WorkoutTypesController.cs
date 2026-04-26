using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernal.Models;
using TrainingService.Application.Interfaces.v1;

namespace TrainingService.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/workout-types")]
[ApiVersion("1.0")]
[Authorize]
public class WorkoutTypesController(IWorkoutTypeService workoutTypeService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await workoutTypeService.GetAllWorkoutTypesAsync();

        return result.ToActionResult();
    }
}
