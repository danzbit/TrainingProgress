using System.Net;
using System.Net.Http.Json;
using Bff.Api.Integration.Infrastructure;
using Bff.Application.Dtos.v1.Requests;
using Bff.Application.Dtos.v1.Responses;
using Shared.Kernal.Errors;
using Shared.Kernal.Headers;
using Shared.Kernal.Results;

namespace Bff.Api.Integration.Controllers.v1;

[TestFixture]
[NonParallelizable]
public class GoalOrchestrationControllerIntegrationTests
{
    private BffApiFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new BffApiFactory();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        _factory.GoalOrchestrator.Reset();
        _client = _factory.CreateAuthenticatedClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    [Test]
    public async Task SaveGoalAndPropagate_WhenOrchestratorSucceeds_Returns200AndPassesIdempotencyKey()
    {
        var goalId = Guid.NewGuid();
        _factory.GoalOrchestrator.Handler = (_, key, _) =>
            Task.FromResult(ResultOfT<SaveGoalSagaResult>.Success(new SaveGoalSagaResult(goalId, key)));

        var command = new SaveGoalSagaCommand(
            "Run 5k",
            "Weekly endurance goal",
            1,
            2,
            5,
            new DateTime(2026, 4, 22, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 22, 0, 0, 0, DateTimeKind.Utc),
            Guid.NewGuid());

        _client.DefaultRequestHeaders.Add(IdempotencyHeaders.IdempotencyKey, "goal-key-456");

        var response = await _client.PostAsJsonAsync("/api/v1/goal-orchestration", command);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(_factory.GoalOrchestrator.CallCount, Is.EqualTo(1));
        Assert.That(_factory.GoalOrchestrator.LastIdempotencyKey, Is.EqualTo("goal-key-456"));
        Assert.That(_factory.GoalOrchestrator.LastCommand, Is.EqualTo(command));

        var payload = await response.Content.ReadAsStringAsync();
        Assert.That(payload, Does.Contain(goalId.ToString()));
        Assert.That(payload, Does.Contain("goal-key-456"));
    }

    [Test]
    public async Task SaveGoalAndPropagate_WhenOrchestratorFails_Returns400WithPayload()
    {
        var goalId = Guid.NewGuid();
        _factory.GoalOrchestrator.Handler = (_, _, _) =>
            Task.FromResult(ResultOfT<SaveGoalSagaResult>.Failure(
                new SaveGoalSagaResult(goalId, "Goal propagation failed"),
                new Error(ErrorCode.UnexpectedError, "Unexpected failure")));

        var response = await _client.PostAsJsonAsync("/api/v1/goal-orchestration", new SaveGoalSagaCommand(
            "Run 5k",
            "Weekly endurance goal",
            1,
            2,
            5,
            DateTime.UtcNow,
            null,
            null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var payload = await response.Content.ReadAsStringAsync();
        Assert.That(payload, Does.Contain(goalId.ToString()));
        Assert.That(payload, Does.Contain("Goal propagation failed"));
    }

    [Test]
    public async Task SaveGoalAndPropagate_WhenAnonymous_Returns401()
    {
        using var anonymousClient = _factory.CreateClient();

        var response = await anonymousClient.PostAsJsonAsync("/api/v1/goal-orchestration", new SaveGoalSagaCommand(
            "Run 5k",
            "Weekly endurance goal",
            1,
            2,
            5,
            DateTime.UtcNow,
            null,
            null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task SaveGoalAndPropagate_WhenRequestIsInvalid_Returns400AndDoesNotCallOrchestrator()
    {
        var invalidCommand = new SaveGoalSagaCommand(
            string.Empty,
            new string('d', 1001),
            -1,
            99,
            0,
            default,
            DateTime.UtcNow.AddDays(-1),
            null);

        var response = await _client.PostAsJsonAsync("/api/v1/goal-orchestration", invalidCommand);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(_factory.GoalOrchestrator.CallCount, Is.EqualTo(0));
    }
}