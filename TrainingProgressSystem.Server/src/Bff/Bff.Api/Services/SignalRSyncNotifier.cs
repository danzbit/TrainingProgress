using Bff.Api.Hubs;
using Bff.Application.Interfaces.v1;
using Microsoft.AspNetCore.SignalR;

namespace Bff.Api.Services;

internal sealed class SignalRSyncNotifier(IHubContext<SyncHub> hubContext) : ISyncNotifier
{
    public Task NotifyWorkoutCreatedAsync(Guid userId, Guid workoutId, CancellationToken ct = default)
        => hubContext.Clients.User(userId.ToString())
            .SendAsync("WorkoutCreated", new { workoutId }, ct);

    public Task NotifyGoalSavedAsync(Guid userId, Guid goalId, CancellationToken ct = default)
        => hubContext.Clients.User(userId.ToString())
            .SendAsync("GoalSaved", new { goalId }, ct);
}
