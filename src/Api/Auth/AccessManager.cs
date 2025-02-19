using Core.Domain;

namespace Api.Auth;

public class AccessManager(AuthorizedUser user)
{
    public bool HasAnyRole(params Role[] roles)
    {
        return roles.Any(role => role == user.Role);
    }
}
