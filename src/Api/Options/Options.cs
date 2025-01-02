using Infrastructure.Options;

namespace Api.Options;

public static class Options
{
    public static IServiceCollection ConfigureOptions(this IServiceCollection services)
    {
        AddOptions<AuthOptions>(services, "Auth");
        AddOptions<DatabaseOptions>(services, "Database");
        return services;
    }

    private static void AddOptions<T>(IServiceCollection services, string sectionName) where T : class
    {
        services.AddOptions<T>()
            .BindConfiguration(sectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}