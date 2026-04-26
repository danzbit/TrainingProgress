using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernal.Models;
using TrainingService.Application.Interfaces.v1;

namespace TrainingService.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/exercise-types")]
[ApiVersion("1.0")]
[Authorize]
public class ExerciseTypesController(IExerciseTypeService exerciseTypeService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await exerciseTypeService.GetAllExerciseTypesAsync();

        return result.ToActionResult();
    }
}
