using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Bff.Api.Hubs;

[Authorize]
public sealed class SyncHub : Hub;
