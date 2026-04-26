using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernal.Models;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Interfaces.v1;

namespace TrainingService.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class UserPreferencesController(
    IUserPreferenceService userPreferenceService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await userPreferenceService.GetPreferenceAsync(ct);
        return result.ToActionResult();
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateUserPreferenceRequest request, CancellationToken ct)
    {
        var result = await userPreferenceService.UpdatePreferenceAsync(request, ct);
        return result.ToActionResult();
    }
}
