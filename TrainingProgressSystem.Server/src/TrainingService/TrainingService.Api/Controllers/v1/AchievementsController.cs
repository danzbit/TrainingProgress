using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernal.Models;
using TrainingService.Application.Interfaces.v1;

namespace TrainingService.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/achievements")]
[ApiVersion("1.0")]
[Authorize]
public class AchievementsController(IAchievementService achievementService) : ControllerBase
{
    [HttpPost("share")]
    public async Task<IActionResult> ShareProgress(CancellationToken ct)
    {
        var result = await achievementService.ShareProgressAsync(ct);

        return result.ToActionResult();
    }

    [HttpGet("shared/{publicUrlKey}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSharedProgress(string publicUrlKey, CancellationToken ct)
    {
        var result = await achievementService.GetSharedProgressAsync(publicUrlKey, ct);

        return result.ToActionResult();
    }
}
