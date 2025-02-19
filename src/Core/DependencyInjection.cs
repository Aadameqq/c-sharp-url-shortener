using Core.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureCoreDependencies(this IServiceCollection services)
    {
        services.AddScoped<ActivateAccountUseCase>();
        services.AddScoped<CreateAccountUseCase>();
        services.AddSingleton<GetAccountFromTokenUseCase>();
        services.AddScoped<GetCurrentAccountUseCase>();
        services.AddScoped<LogInUseCase>();
        services.AddScoped<LogOutUseCase>();
        services.AddScoped<RefreshTokensUseCase>();
        services.AddScoped<InitializePasswordResetUseCase>();
        services.AddScoped<AssignRoleUseCase>();
        services.AddScoped<UnassignRoleUseCase>();
        services.AddScoped<ListRolesUseCase>();
        services.AddScoped<ResetPasswordUseCase>();
        return services;
    }
}
