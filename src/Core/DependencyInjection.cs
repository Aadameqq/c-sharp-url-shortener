using Core.Interactors;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureCoreDependencies(this IServiceCollection services)
    {
        services.AddScoped<AccountInteractor>();
        services.AddScoped<AuthInteractor>();
        services.AddSingleton<AccessTokenInteractor>();
        return services;
    }
}
