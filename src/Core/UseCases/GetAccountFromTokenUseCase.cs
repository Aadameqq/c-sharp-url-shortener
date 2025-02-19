using Core.Domain;
using Core.Dtos;
using Core.Exceptions;
using Core.Ports;

namespace Core.UseCases;

public class GetAccountFromTokenUseCase(TokenService tokenService)
{
    public async Task<Result<AccessTokenPayload>> Execute(string accessToken)
    {
        var payload = await tokenService.FetchPayloadIfValid(accessToken);

        if (payload is null)
        {
            return new InvalidToken();
        }

        return payload;
    }
}
