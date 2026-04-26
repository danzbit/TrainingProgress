using AnalyticsService.Api.Controllers.v1;
using AnalyticsService.Application.Dtos.v1.Responses;
using AnalyticsService.Application.Interfaces.v1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace AnalyticsService.Api.Tests.Controllers.v1;

[TestFixture]
public class WorkoutAnalyticsControllerTests
{
    private Mock<IWorkoutAnalyticsService> _serviceMock = null!;
    private WorkoutAnalyticsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IWorkoutAnalyticsService>(MockBehavior.Strict);
        var logger = Mock.Of<ILogger<WorkoutAnalyticsController>>();
        _controller = new WorkoutAnalyticsController(_serviceMock.Object, logger);
    }

    [Test]
    public async Task GetSummary_WhenServiceSucceeds_ReturnsOkObjectResult()
    {
        var expected = new WorkoutSummaryResponse { AmountPerWeek = 4, WeekDurationMin = 180 };
        _serviceMock
            .Setup(s => s.GetSummaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<WorkoutSummaryResponse>.Success(expected));

        var result = await _controller.GetSummary(CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(expected));
        _serviceMock.Verify(s => s.GetSummaryAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetSummary_WhenServiceReturnsNotFound_ReturnsNotFoundObjectResult()
    {
        _serviceMock
            .Setup(s => s.GetSummaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<WorkoutSummaryResponse>.Failure(
                new Error(ErrorCode.EntityNotFound, "Not found")));

        var result = await _controller.GetSummary(CancellationToken.None);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetLast7DaysActivity_PassesSevenDaysToService()
    {
        _serviceMock
            .Setup(s => s.GetDailyTrendAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<IReadOnlyList<WorkoutDailyTrendPointResponse>>.Success([]));

        var result = await _controller.GetLast7DaysActivity(CancellationToken.None);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        _serviceMock.Verify(s => s.GetDailyTrendAsync(7, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetCountsByType_PassesFromAndToToService()
    {
        var from = new DateTime(2026, 4, 1);
        var to = new DateTime(2026, 4, 21);

        _serviceMock
            .Setup(s => s.GetCountByTypeAsync(from, to, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<IReadOnlyList<WorkoutCountByTypeResponse>>.Success([]));

        var result = await _controller.GetCountsByType(from, to, CancellationToken.None);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        _serviceMock.Verify(s => s.GetCountByTypeAsync(from, to, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetStatisticsOverview_WhenServiceFailsWithDownstreamUnavailable_Returns503()
    {
        _serviceMock
            .Setup(s => s.GetStatisticsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<WorkoutStatisticsOverviewResponse>.Failure(
                new Error(ErrorCode.DownstreamServiceUnavailable, "Downstream unavailable")));

        var result = await _controller.GetStatisticsOverview(CancellationToken.None);

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(503));
    }
}
