using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernal.Models;
using AnalyticsService.Application.Interfaces.v1;
using Microsoft.Extensions.Logging;

namespace AnalyticsService.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/profile-analytics")]
[ApiVersion("1.0")]
[Authorize]
public class ProfileAnalyticsController(
    IProfileAnalyticsService profileAnalyticsService,
    ILogger<ProfileAnalyticsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProfileAnalytics(CancellationToken ct)
    {
        logger.LogInformation("Request received for profile analytics");

        var result = await profileAnalyticsService.GetProfileAnalyticsAsync(ct);

        logger.LogInformation("Profile analytics request completed. IsFailure: {IsFailure}", result.IsFailure);

        return result.ToActionResult();
    }
}