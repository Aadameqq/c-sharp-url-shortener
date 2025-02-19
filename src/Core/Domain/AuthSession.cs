using Core.Exceptions;

namespace Core.Domain;

public class AuthSession
{
    private DateTime expiresAt;
    private readonly TimeSpan lifeSpan = TimeSpan.FromMinutes(30);

    public AuthSession(Guid userId, DateTime now)
    {
        UserId = userId;
        expiresAt = now + lifeSpan;
        CurrentToken = new RefreshToken(Id);
    }

    private AuthSession() { }

    public Guid Id { get; init; } = Guid.NewGuid();
    public RefreshToken CurrentToken { get; private set; }
    public Guid UserId { get; }

    public Result<RefreshToken> Refresh(DateTime now)
    {
        if (!IsActive(now))
        {
            return new SessionInactive();
        }

        CurrentToken = new RefreshToken(Id);

        expiresAt = now + lifeSpan;

        return CurrentToken;
    }

    public bool IsActive(DateTime now)
    {
        return now < expiresAt;
    }
}
