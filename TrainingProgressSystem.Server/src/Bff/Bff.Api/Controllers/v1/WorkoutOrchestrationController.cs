using Bff.Application.Dtos.v1.Requests;
using Bff.Application.Interfaces.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernal.Headers;

namespace Bff.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/workout-orchestration")]
[ApiVersion("1.0")]
[Authorize]
public class WorkoutOrchestrationController(ICreateWorkoutSagaOrchestrator orchestrator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateWorkoutAndPropagate([FromBody] CreateWorkoutSagaCommand command,
        CancellationToken ct)
    {
        var idempotencyKey = HttpContext.Request.Headers.TryGetValue(IdempotencyHeaders.IdempotencyKey, out var headerValue)
            ? headerValue.ToString()
            : null;

        var result = await orchestrator.ExecuteAsync(command, idempotencyKey, ct);

        var workoutId = result.Value?.WorkoutId;
        var error = result.Value?.Error ?? (result.IsFailure ? result.Error.Description : null);

        if (result.IsFailure)
            return BadRequest(new { workoutId, error });

        return Ok(new { workoutId, error });
    }
}
