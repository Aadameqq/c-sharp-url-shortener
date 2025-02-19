namespace Core.Domain;

public record RefreshToken(Guid Id, Guid SessionId)
{
    public RefreshToken(Guid sessionId) : this(Guid.NewGuid(), sessionId) { }
}
