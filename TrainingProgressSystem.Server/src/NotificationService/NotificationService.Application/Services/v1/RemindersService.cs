using NotificationService.Application.Dtos.v1.Responses;
using NotificationService.Application.Interfaces.v1;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces;
using Shared.Abstractions.Auth;
using Shared.Kernal.Results;

namespace NotificationService.Application.Services.v1;

public class RemindersService(IRemindersRepository remindersRepository, ICurrentUser currentUser) : IRemindersService
{

    public ResultOfT<IReadOnlyList<ReminderResponse>> GetAllRemindersAsync()
    {
        var userId = currentUser.GetCurrentUserId();
        
        var goalReminders = remindersRepository.GetGoalReminders(userId.Value);
        var goalMessages = goalReminders.Select(GenerateGoalReminderMessage).ToList();

        return ResultOfT<IReadOnlyList<ReminderResponse>>.Success(goalMessages);
    }

    private ReminderResponse GenerateGoalReminderMessage(GoalReminder goal)
    {
        var unit = GetUnitForMetricType(goal.MetricType);
        var period = GetPeriodDescription(goal.PeriodType, goal.EndDate);
        var message = $"📌 You need {goal.Remaining} more {unit} to reach your \"{goal.Name}\" goal {period}.";
        return new ReminderResponse(message);
    }

    private static string GetUnitForMetricType(int metricType)
    {
        return metricType switch
        {
            0 => "workouts", // WorkoutCount
            1 => "minutes", // TotalDurationMinutes
            2 => "km", // DistanceKm
            3 => "calories", // CaloriesBurned
            4 => "days", // StreakDays
            5 => "workout types", // UniqueWorkoutTypes
            6 => "workouts", // WeekendWorkouts
            7 => "workouts", // MorningWorkouts
            _ => "units"
        };
    }

    private static string GetPeriodDescription(int periodType, DateTime? endDate)
    {
        return periodType switch
        {
            0 => "this week", // Weekly
            1 => "this month", // Monthly
            2 => endDate.HasValue ? $"by {endDate.Value:d}" : "in the custom period", // CustomRange
            3 => "in the next 7 days", // RollingWindow
            _ => "in the period"
        };
    }
}