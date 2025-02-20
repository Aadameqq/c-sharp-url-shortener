using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options;

public class AccountOptions
{
    [Required]
    public required int ActivationCodeLifeSpanInMinutes { get; init; }

    [Required]
    public required int PasswordResetCodeLifeSpanInMinutes { get; init; }
}
