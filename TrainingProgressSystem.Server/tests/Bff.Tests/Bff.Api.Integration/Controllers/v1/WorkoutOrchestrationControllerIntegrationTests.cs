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
public class WorkoutOrchestrationControllerIntegrationTests
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
        _factory.WorkoutOrchestrator.Reset();
        _client = _factory.CreateAuthenticatedClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    [Test]
    public async Task CreateWorkoutAndPropagate_WhenOrchestratorSucceeds_Returns200AndPassesIdempotencyKey()
    {
        var workoutId = Guid.NewGuid();
        _factory.WorkoutOrchestrator.Handler = (_, key, _) =>
            Task.FromResult(ResultOfT<CreateWorkoutSagaResult>.Success(new CreateWorkoutSagaResult(workoutId, key)));

        var command = new CreateWorkoutSagaCommand(
            new DateTime(2026, 4, 22, 10, 0, 0, DateTimeKind.Utc),
            Guid.NewGuid(),
            45,
            "Leg day",
            [new CreateWorkoutExerciseSagaCommand(Guid.NewGuid(), Guid.NewGuid(), 3, 10, null, 60)],
            Guid.NewGuid());

        _client.DefaultRequestHeaders.Add(IdempotencyHeaders.IdempotencyKey, "workout-key-123");

        var response = await _client.PostAsJsonAsync("/api/v1/workout-orchestration", command);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(_factory.WorkoutOrchestrator.CallCount, Is.EqualTo(1));
        Assert.That(_factory.WorkoutOrchestrator.LastIdempotencyKey, Is.EqualTo("workout-key-123"));
        
        // Verify command properties (exercises list type differs due to JSON deserialization)
        var lastCmd = _factory.WorkoutOrchestrator.LastCommand;
        Assert.That(lastCmd, Is.Not.Null);
        
        // Use null-forgiving operator once after null check
        var cmd = lastCmd!;
        Assert.That(cmd.Date, Is.EqualTo(command.Date));
        Assert.That(cmd.WorkoutTypeId, Is.EqualTo(command.WorkoutTypeId));
        Assert.That(cmd.DurationMin, Is.EqualTo(command.DurationMin));
        Assert.That(cmd.Notes, Is.EqualTo(command.Notes));
        Assert.That(cmd.Exercises, Is.Not.Null);
#pragma warning disable CS8602 // NUnit assertions don't provide nullability flow analysis
        Assert.That(cmd.Exercises.Count, Is.EqualTo(command.Exercises.Count));
#pragma warning restore CS8602
        Assert.That(cmd.CorrelationId, Is.EqualTo(command.CorrelationId));

        var payload = await response.Content.ReadAsStringAsync();
        Assert.That(payload, Is.Not.Null);
        Assert.That(payload.Contains(workoutId.ToString()));
        Assert.That(payload.Contains("workout-key-123"));
    }

    [Test]
    public async Task CreateWorkoutAndPropagate_WhenOrchestratorFails_Returns400WithPayload()
    {
        var workoutId = Guid.NewGuid();
        _factory.WorkoutOrchestrator.Handler = (_, _, _) =>
            Task.FromResult(ResultOfT<CreateWorkoutSagaResult>.Failure(
                new CreateWorkoutSagaResult(workoutId, "Workout propagation failed"),
                new Error(ErrorCode.UnexpectedError, "Unexpected failure")));

        var response = await _client.PostAsJsonAsync("/api/v1/workout-orchestration", new CreateWorkoutSagaCommand(
            DateTime.UtcNow,
            Guid.NewGuid(),
            30,
            null,
            null,
            null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var payload = await response.Content.ReadAsStringAsync();
        Assert.That(payload, Does.Contain(workoutId.ToString()));
        Assert.That(payload, Does.Contain("Workout propagation failed"));
    }

    [Test]
    public async Task CreateWorkoutAndPropagate_WhenAnonymous_Returns401()
    {
        using var anonymousClient = _factory.CreateClient();

        var response = await anonymousClient.PostAsJsonAsync("/api/v1/workout-orchestration", new CreateWorkoutSagaCommand(
            DateTime.UtcNow,
            Guid.NewGuid(),
            30,
            null,
            null,
            null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task CreateWorkoutAndPropagate_WhenRequestIsInvalid_Returns400AndDoesNotCallOrchestrator()
    {
        var invalidCommand = new CreateWorkoutSagaCommand(
            default,
            Guid.Empty,
            0,
            new string('a', 1001),
            [new CreateWorkoutExerciseSagaCommand(null, Guid.Empty, 0, 0, 0, -1)],
            null);

        var response = await _client.PostAsJsonAsync("/api/v1/workout-orchestration", invalidCommand);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(_factory.WorkoutOrchestrator.CallCount, Is.EqualTo(0));
    }
}