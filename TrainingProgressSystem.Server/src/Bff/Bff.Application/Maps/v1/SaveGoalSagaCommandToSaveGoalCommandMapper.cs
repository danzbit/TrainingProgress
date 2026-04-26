using Bff.Application.Dtos.v1.Commands;
using Bff.Application.Dtos.v1.Requests;

namespace Bff.Application.Maps.v1;

public static class SaveGoalSagaCommandToSaveGoalCommandMapper
{
    public static SaveGoalCommand ToSaveGoalCommand(this SaveGoalSagaCommand command, Guid userId)
    {
        return new SaveGoalCommand(
            userId,
            command.Name,
            command.Description,
            command.MetricType,
            command.PeriodType,
            command.TargetValue,
            command.StartDate,
            command.EndDate);
    }
}
