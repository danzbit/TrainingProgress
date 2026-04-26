using NotificationService.Application.Dtos.v1.Responses;
using Shared.Kernal.Results;

namespace NotificationService.Application.Interfaces.v1;

public interface IRemindersService
{
   ResultOfT<IReadOnlyList<ReminderResponse>> GetAllRemindersAsync();
}