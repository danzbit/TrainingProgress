using NotificationService.Application.Dtos.v1.Responses;
using NotificationService.Application.Interfaces.v1;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace NotificationService.Api.Integration.Infrastructure;

public sealed class StubRemindersService : IRemindersService
{
    public int GetAllRemindersCallCount { get; private set; }
    public List<ReminderResponse> ReminderResponses { get; set; } = new();
    public Error? ExpectedError { get; set; }

    public ResultOfT<IReadOnlyList<ReminderResponse>> GetAllRemindersAsync()
    {
        GetAllRemindersCallCount++;

        if (ExpectedError is not null)
        {
            return ResultOfT<IReadOnlyList<ReminderResponse>>.Failure(
                ReminderResponses.AsReadOnly(),
                ExpectedError);
        }

        return ResultOfT<IReadOnlyList<ReminderResponse>>.Success(ReminderResponses.AsReadOnly());
    }

    public void Reset()
    {
        GetAllRemindersCallCount = 0;
        ReminderResponses.Clear();
        ExpectedError = null;
    }
}
