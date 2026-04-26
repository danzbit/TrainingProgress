using Bff.Application.Dtos.v1.Requests;
using Bff.Application.Interfaces.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernal.Headers;

namespace Bff.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/goal-orchestration")]
[ApiVersion("1.0")]
[Authorize]
public class GoalOrchestrationController(ISaveGoalSagaOrchestrator orchestrator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SaveGoalAndPropagate([FromBody] SaveGoalSagaCommand command, CancellationToken ct)
    {
        var idempotencyKey = HttpContext.Request.Headers.TryGetValue(IdempotencyHeaders.IdempotencyKey, out var headerValue)
            ? headerValue.ToString()
            : null;

        var result = await orchestrator.ExecuteAsync(command, idempotencyKey, ct);

        var goalId = result.Value?.GoalId;
        var error = result.Value?.Error ?? (result.IsFailure ? result.Error.Description : null);

        if (result.IsFailure)
            return BadRequest(new { goalId, error });

        return Ok(new { goalId, error });
    }
}
