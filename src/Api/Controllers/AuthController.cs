using Api.Auth;
using Api.Controllers.Dtos;
using Api.Dtos;
using Core.Domain;
using Core.Exceptions;
using Core.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(
    LogInUseCase logInUseCase,
    LogOutUseCase logOutUseCase,
    RefreshTokensUseCase refreshTokensUseCase
) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TokenPairResponse>> LogIn([FromBody] LogInBody body)
    {
        var result = await logInUseCase.Execute(body.Email, body.Password);

        if (result.IsFailure)
        {
            return result.Exception switch
            {
                NoSuch<Account> _ => ApiResponse.Unauthorized(),
                InvalidCredentials _ => ApiResponse.Unauthorized(),
                AccountNotActivated _ => ApiResponse.Unauthorized(
                    "Account has not been activated yet"
                ),
                _ => throw result.Exception,
            };
        }

        return new TokenPairResponse(result.Value.AccessToken, result.Value.RefreshToken);
    }

    [HttpDelete]
    [RequireAuth]
    public async Task<IActionResult> LogOut([FromAuth] AuthorizedUser authUser)
    {
        var result = await logOutUseCase.Execute(authUser.UserId, authUser.SessionId);

        if (result is { IsFailure: true, Exception: NoSuch<Account> })
        {
            return ApiResponse.Unauthorized();
        }

        return ApiResponse.Ok();
    }

    [HttpPut]
    public async Task<ActionResult<TokenPairResponse>> RefreshTokens(
        [FromBody] RefreshTokensBody body
    )
    {
        var result = await refreshTokensUseCase.Execute(body.RefreshToken);

        if (result.IsFailure)
        {
            return result.Exception switch
            {
                NoSuch<Account> _ => ApiResponse.Unauthorized(),
                NoSuch<AuthSession> _ => ApiResponse.NotFound(),
                InvalidToken _ => ApiResponse.NotFound(),
                _ => throw result.Exception,
            };
        }

        return new TokenPairResponse(result.Value.AccessToken, result.Value.RefreshToken);
    }
}
