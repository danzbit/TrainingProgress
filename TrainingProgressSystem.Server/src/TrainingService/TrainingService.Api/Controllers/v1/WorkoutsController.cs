using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api.OData;
using Shared.Kernal.Models;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Interfaces.v1;

namespace TrainingService.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class WorkoutsController(IWorkoutService workoutService) : ControllerBase
{
    [HttpGet]
    [EnableStableODataQuery]
    public async Task<IActionResult> GetAll()
    {
        var result = await workoutService.GetAllWorkoutsAsync();

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        return Ok(result.Value.AsQueryable());
    }

    [HttpGet("{workoutId:guid}")]
    public async Task<IActionResult> GetById(Guid workoutId)
    {
        var result = await workoutService.GetWorkoutAsync(workoutId);

        return result.ToActionResult();
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateWorkoutRequest request)
    {
        var result = await workoutService.UpdateWorkoutAsync(request);

        return result.ToActionResult();
    }

    [HttpDelete("{workoutId:guid}")]
    public async Task<IActionResult> Delete(Guid workoutId)
    {
        var result = await workoutService.DeleteWorkoutAsync(workoutId);

        return result.ToActionResult();
    }
}