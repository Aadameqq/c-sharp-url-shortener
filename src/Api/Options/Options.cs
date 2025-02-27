using Infrastructure.Options;

namespace Api.Options;

public static class Options
{
    public static IServiceCollection ConfigureOptions(
        this IServiceCollection services,
        IHostEnvironment environment
    )
    {
        AddOptions<AuthOptions>(services, "Auth");
        AddOptions<DatabaseOptions>(services, "Database");
        AddOptions<SmtpOptions>(services, "Smtp");
        AddOptions<RedisOptions>(services, "Redis");
        AddOptions<AccountOptions>(services, "Account");

        services.PostConfigure<StorageOptions>(options =>
        {
            options.Location = Path.Combine(environment.ContentRootPath, "wwwroot", "problems");
        });

        return services;
    }

    private static void AddOptions<T>(IServiceCollection services, string sectionName)
        where T : class
    {
        services
            .AddOptions<T>()
            .BindConfiguration(sectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}
