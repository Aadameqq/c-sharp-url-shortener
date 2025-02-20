using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.Auth;

[AttributeUsage(AttributeTargets.Parameter)]
public class FromAuthAttribute : Attribute, IBindingSourceMetadata
{
    public BindingSource BindingSource => BindingSource.Special;
}
