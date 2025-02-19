using Core.Domain;

namespace Core.Dtos;

public record RefreshTokenPayload(Guid AccountId, Guid TokenId, Guid SessionId)
{
    public RefreshToken ToRefreshToken()
    {
        return new RefreshToken(TokenId, SessionId);
    }
}
