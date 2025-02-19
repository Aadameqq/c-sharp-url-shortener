using Core.Exceptions;

namespace Core.Domain;

public class AuthSession
{
    private readonly TimeSpan lifeSpan = TimeSpan.FromMinutes(30);

    private Guid currentTokenId;
    private DateTime expiresAt;

    public AuthSession(Guid userId, DateTime now)
    {
        UserId = userId;
        expiresAt = now + lifeSpan;
        currentTokenId = Guid.NewGuid();
    }

    private AuthSession() { }

    public Guid Id { get; init; } = Guid.NewGuid();

    public RefreshToken CurrentToken => new(currentTokenId, Id);

    public Guid UserId { get; }

    public Result<RefreshToken> Refresh(DateTime now)
    {
        if (!IsActive(now))
        {
            return new SessionInactive();
        }

        currentTokenId = Guid.NewGuid();

        expiresAt = now + lifeSpan;

        return CurrentToken;
    }

    public bool IsActive(DateTime now)
    {
        return now < expiresAt;
    }
}
