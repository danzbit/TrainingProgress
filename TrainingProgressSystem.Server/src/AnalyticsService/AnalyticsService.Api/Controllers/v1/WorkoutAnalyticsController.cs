using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernal.Models;
using AnalyticsService.Application.Interfaces.v1;
using Microsoft.Extensions.Logging;

namespace AnalyticsService.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/workout-analytics")]
[ApiVersion("1.0")]
[Authorize]
public class WorkoutAnalyticsController(
   IWorkoutAnalyticsService workoutAnalyticsService,
   ILogger<WorkoutAnalyticsController> logger) : ControllerBase
{
   [HttpGet("summary")]
   public async Task<IActionResult> GetSummary(CancellationToken ct)
   {
      logger.LogInformation("Request received for workout analytics summary");

      var result = await workoutAnalyticsService.GetSummaryAsync(ct);

      logger.LogInformation("Workout analytics summary request completed. IsFailure: {IsFailure}", result.IsFailure);

      return result.ToActionResult();
   }

   [HttpGet("daily/last-7-days")]
   public async Task<IActionResult> GetLast7DaysActivity(CancellationToken ct = default)
   {
      logger.LogInformation("Request received for last 7 days workout activity");

      var result = await workoutAnalyticsService.GetDailyTrendAsync(7, ct);

      logger.LogInformation("Last 7 days workout activity request completed. IsFailure: {IsFailure}",
         result.IsFailure);

      return result.ToActionResult();
   }

   [HttpGet("by-type")]
   public async Task<IActionResult> GetCountsByType([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null,
      CancellationToken ct = default)
   {
      logger.LogInformation("Request received for workout counts by type. From: {From}, To: {To}", from, to);

      var result = await workoutAnalyticsService.GetCountByTypeAsync(from, to, ct);

      logger.LogInformation("Workout counts by type request completed. IsFailure: {IsFailure}", result.IsFailure);

      return result.ToActionResult();
   }

   [HttpGet("statistics-overview")]
   public async Task<IActionResult> GetStatisticsOverview(CancellationToken ct = default)
   {
      logger.LogInformation("Request received for workout statistics overview");

      var result = await workoutAnalyticsService.GetStatisticsOverviewAsync(ct);

      logger.LogInformation("Workout statistics overview request completed. IsFailure: {IsFailure}",
         result.IsFailure);

      return result.ToActionResult();
   }
}