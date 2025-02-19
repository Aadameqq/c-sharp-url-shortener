using Core.Domain;

namespace Core.UseCases;

public class ListRolesUseCase
{
    public Result<List<Role>> Execute()
    {
        return Role.Roles;
    }
}
