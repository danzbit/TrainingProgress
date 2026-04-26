using Shared.Kernal.Results;

namespace Shared.Abstractions.Auth;

public interface ICurrentUser
{
    public ResultOfT<Guid> GetCurrentUserId();
}