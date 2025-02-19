using Api.Auth;
using Api.Controllers.Dtos;
using Core.Domain;
using Core.Exceptions;
using Core.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountsController(
    CreateAccountUseCase createAccountUseCase,
    GetCurrentAccountUseCase getCurrentAccountUseCase,
    InitializePasswordResetUseCase initializePasswordResetUseCase,
    ActivateAccountUseCase activateAccountUseCase,
    ResetPasswordUseCase resetPasswordUseCase,
    AssignRoleUseCase assignRoleUseCase,
    UnassignRoleUseCase unassignRoleUseCase
) : ControllerBase
{
    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] CreateAccountBody body)
    {
        var result = await createAccountUseCase.Execute(body.UserName, body.Email, body.Password);

        if (result is { IsFailure: true, Exception: AlreadyExists<Account> })
        {
            return ApiResponse.Conflict("Account with given email already exists");
        }

        return ApiResponse.Ok();
    }

    [HttpGet("@me")]
    [RequireAuth]
    public async Task<ActionResult<GetAuthenticatedUserResponse>> GetAuthenticated(
        [FromAuth] AuthorizedUser user
    )
    {
        var result = await getCurrentAccountUseCase.Execute(user.UserId);

        if (result is { IsFailure: true, Exception: NoSuch<Account> })
        {
            return ApiResponse.Unauthorized();
        }

        return new GetAuthenticatedUserResponse(result.Value);
    }

    [HttpDelete("@me/password")]
    public async Task<IActionResult> InitializeResetPassword(
        [FromBody] InitializeResetPasswordBody body
    )
    {
        var result = await initializePasswordResetUseCase.Execute(body.Email);

        if (result is { IsFailure: true, Exception: NoSuch<Account> })
        {
            return ApiResponse.NotFound();
        }

        return ApiResponse.Ok("Password reset email sent");
    }

    [HttpGet("@me/activation/{code}")]
    public async Task<IActionResult> Activate([FromRoute] string code)
    {
        var result = await activateAccountUseCase.Execute(code);

        if (result.IsFailure)
        {
            return result.Exception switch
            {
                NoSuch<Account> _ => ApiResponse.NotFound(),
                NoSuch _ => ApiResponse.NotFound(),
                _ => throw result.Exception
            };
        }

        return ApiResponse.Ok("Account activated");
    }

    [HttpDelete("@me/password/{code}")]
    public async Task<IActionResult> ResetPassword(
        [FromRoute] string code,
        [FromBody] ResetPasswordBody body
    )
    {
        var result = await resetPasswordUseCase.Execute(code, body.NewPassword);

        if (result is { IsFailure: true, Exception: NoSuch })
        {
            return ApiResponse.NotFound();
        }

        return ApiResponse.Ok();
    }

    [HttpPost("{accountId}/role")]
    [RequireAuth]
    public async Task<IActionResult> AssignRole(
        [FromAuth] AuthorizedUser issuer,
        [FromRoute] string accountId,
        [FromBody] AssignRoleBody body,
        [FromAuth] AccessManager accessManager
    )
    {
        if (!accessManager.HasAnyRole(Role.Admin))
        {
            return ApiResponse.Forbid();
        }

        if (!Guid.TryParse(accountId, out var parsedAccountId))
        {
            return ApiResponse.NotFound("Account not found");
        }

        if (!Role.TryParse(body.RoleName, out var role))
        {
            return ApiResponse.NotFound("Role not found");
        }

        var result = await assignRoleUseCase.Execute(issuer.UserId, parsedAccountId, role);

        if (result.IsFailure)
        {
            return result.Exception switch
            {
                NoSuch<Account> _ => ApiResponse.NotFound("Account not found"),
                CannotManageOwn<Role> _ => ApiResponse.Forbid(
                    "Assigning a role to your own account is not permitted"
                ),
                RoleAlreadyAssigned _ => ApiResponse.Conflict(
                    "Account already assigned to role. Remove role before assigning"
                ),
                _ => throw result.Exception
            };
        }

        return ApiResponse.Ok();
    }

    [HttpDelete("{accountId}/role")]
    [RequireAuth]
    public async Task<IActionResult> UnassignRole(
        [FromAuth] AuthorizedUser issuer,
        [FromRoute] string accountId,
        [FromAuth] AccessManager accessManager
    )
    {
        if (!accessManager.HasAnyRole(Role.Admin))
        {
            return ApiResponse.Forbid();
        }

        if (!Guid.TryParse(accountId, out var parsedAccountId))
        {
            return ApiResponse.NotFound();
        }

        var result = await unassignRoleUseCase.Execute(issuer.UserId, parsedAccountId);

        if (result.IsFailure)
        {
            return result.Exception switch
            {
                NoSuch<Account> _ => ApiResponse.NotFound(),
                CannotManageOwn<Role> _ => ApiResponse.Forbid(
                    "Unassigning a role from your own account is not permitted"
                ),
                _ => throw result.Exception
            };
        }

        return ApiResponse.Ok();
    }
}
