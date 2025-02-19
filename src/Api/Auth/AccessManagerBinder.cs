using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.Auth;

public class AccessManagerBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (
            bindingContext.HttpContext.Items.TryGetValue("authorizedUser", out var value)
            && value is AuthorizedUser authUser
        )
        {
            bindingContext.Result = ModelBindingResult.Success(new AccessManager(authUser));
        }
        else
        {
            throw new InvalidOperationException(
                $"{nameof(AccessManager)} cannot be used without authorization."
            );
        }

        return Task.CompletedTask;
    }
}

public class AccessManagerBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        return context.Metadata.ModelType == typeof(AccessManager)
            ? new AccessManagerBinder()
            : null;
    }
}
