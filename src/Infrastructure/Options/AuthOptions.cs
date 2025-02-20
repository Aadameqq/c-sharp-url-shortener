using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options;

public class AuthOptions
{
    [Required]
    public required string AccessTokenSecret { get; init; }

    [Required]
    public required string RefreshTokenSecret { get; init; }

    [Required]
    public required int AccessTokenLifetimeInMinutes { get; init; }

    [Required]
    public required string Issuer { get; init; }
}
