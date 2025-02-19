using Core.Domain;

namespace Api.Controllers.Dtos;

public record GetAllRolesResponse
{
    public GetAllRolesResponse(List<Role> roles)
    {
        Roles = roles.Select(r => r.Name).ToList();
    }

    public List<string> Roles { get; private init; }
}
