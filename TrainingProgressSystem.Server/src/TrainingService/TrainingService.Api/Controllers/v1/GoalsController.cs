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
public class GoalsController(IGoalService goalService) : ControllerBase
{
    [HttpGet]
    [EnableStableODataQuery]
    public async Task<IActionResult> GetAll()
    {
        var result = await goalService.GetAllGoalsAsync();

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        return Ok(result.Value.AsQueryable());
    }

    [HttpGet("{goalId:guid}")]
    public async Task<IActionResult> GetById(Guid goalId)
    {
        var result = await goalService.GetGoalAsync(goalId);

        return result.ToActionResult();
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateGoalRequest request)
    {
        var result = await goalService.UpdateGoalAsync(request);

        return result.ToActionResult();
    }

    [HttpDelete("{goalId:guid}")]
    public async Task<IActionResult> Delete(Guid goalId)
    {
        var result = await goalService.DeleteGoalAsync(goalId);

        return result.ToActionResult();
    }
}
