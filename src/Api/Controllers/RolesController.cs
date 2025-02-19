using Api.Auth;
using Api.Controllers.Dtos;
using Core.Domain;
using Core.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RolesController(ListRolesUseCase listRolesUseCase) : ControllerBase
{
    [HttpGet]
    [RequireAuth]
    public ActionResult<GetAllRolesResponse> GetAll([FromAuth] AccessManager accessManager)
    {
        if (!accessManager.HasAnyRole(Role.Admin))
        {
            return ApiResponse.Forbid();
        }

        var result = listRolesUseCase.Execute();

        return new GetAllRolesResponse(result.Value);
    }
}
