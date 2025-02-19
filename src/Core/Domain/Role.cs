namespace Core.Domain;

public record Role
{
    public static readonly Role Admin;
    public static readonly Role ProblemsCreator;
    public static readonly Role None;

    static Role()
    {
        Admin = Register(nameof(Admin));
        ProblemsCreator = Register(nameof(ProblemsCreator));
        None = Register(nameof(None));
    }

    private Role(string name)
    {
        Name = name;
    }

    public static List<Role> Roles { get; } = [];

    public string Name { get; }

    public static bool TryParse(string name, out Role role)
    {
        role = null;
        var found = Roles.Find(r => r.Name == name);

        if (found is null)
        {
            return false;
        }

        role = found;
        return true;
    }

    public static Role ParseOrFail(string name)
    {
        return TryParse(name, out var role)
            ? role
            : throw new InvalidOperationException($"Invalid role name '{name}'");
    }

    private static Role Register(string name)
    {
        var role = new Role(name);
        Roles.Add(role);
        return role;
    }
}
