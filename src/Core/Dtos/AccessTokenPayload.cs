using Core.Domain;

namespace Core.Dtos;

public record AccessTokenPayload(Guid UserId, Guid SessionId, Role Role);
