using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Domain;
using Core.Dtos;
using Core.Ports;
using Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Other;

public class SystemTokenService(IOptions<AuthOptions> authOptions) : TokenService
{
    private const string SessionIdClaimType = "sessionId";
    private const string TokenIdClaimType = "tokenId";

    private readonly SymmetricSecurityKey accessTokenSigningKey = new(
        Encoding.UTF8.GetBytes(authOptions.Value.AccessTokenSecret)
    );

    private readonly SymmetricSecurityKey refreshTokenSigningKey = new(
        Encoding.UTF8.GetBytes(authOptions.Value.RefreshTokenSecret)
    );

    public TokenPairOutput CreateTokenPair(Account account, Guid sessionId, Guid tokenId)
    {
        var accessToken = CreateAccessToken(account, sessionId);
        var refreshToken = CreateRefreshToken(account, sessionId, tokenId);

        return new TokenPairOutput(accessToken, refreshToken);
    }

    public async Task<AccessTokenPayload?> FetchPayloadIfValid(string accessToken)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidIssuer = authOptions.Value.Issuer,
            ValidateAudience = false,
            IssuerSigningKey = accessTokenSigningKey
        };
        try
        {
            var principal = await Task.Run(() =>
                new JwtSecurityTokenHandler().ValidateToken(
                    accessToken,
                    validationParameters,
                    out var token
                )
            );

            if (principal is null)
            {
                return null;
            }

            var userId = Guid.Parse(GetClaim(principal, ClaimTypes.NameIdentifier));
            var sessionId = Guid.Parse(GetClaim(principal, SessionIdClaimType));
            var role = GetClaim(principal, ClaimTypes.Role);

            return new AccessTokenPayload(userId, sessionId, Role.ParseOrFail(role));
        }
        catch
        {
            return null;
        }
    }

    public async Task<RefreshTokenPayload?> FetchRefreshTokenPayloadIfValid(string refreshToken)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidIssuer = authOptions.Value.Issuer,
            ValidateAudience = false,
            IssuerSigningKey = refreshTokenSigningKey
        };
        try
        {
            var principal = await Task.Run(() =>
                new JwtSecurityTokenHandler().ValidateToken(
                    refreshToken,
                    validationParameters,
                    out var token
                )
            );

            if (principal is null)
            {
                return null;
            }

            var accountId = Guid.Parse(GetClaim(principal, ClaimTypes.NameIdentifier));
            var sessionId = Guid.Parse(GetClaim(principal, SessionIdClaimType));
            var tokenId = Guid.Parse(GetClaim(principal, TokenIdClaimType));

            return new RefreshTokenPayload(accountId, tokenId, sessionId);
        }
        catch
        {
            return null;
        }
    }

    private static string GetClaim(ClaimsPrincipal principal, string claimType)
    {
        var claim = principal.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        if (claim is null)
        {
            throw new InvalidOperationException("No claim found for claim type");
        }

        return claim;
    }

    private string CreateAccessToken(Account account, Guid sessionId)
    {
        var accessTokenClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(ClaimTypes.Role, account.Role.Name),
            new(SessionIdClaimType, sessionId.ToString())
        };

        return GenerateToken(
            accessTokenClaims,
            authOptions.Value.Issuer,
            authOptions.Value.AccessTokenLifetimeInMinutes,
            accessTokenSigningKey
        );
    }

    private string CreateRefreshToken(Account account, Guid sessionId, Guid tokenId)
    {
        var refreshTokenClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(SessionIdClaimType, sessionId.ToString()),
            new(TokenIdClaimType, tokenId.ToString())
        };

        return GenerateToken(
            refreshTokenClaims,
            authOptions.Value.Issuer,
            0,
            refreshTokenSigningKey
        );
    }


    private string GenerateToken(
        List<Claim> claims,
        string issuer,
        int lifetimeInMinutes,
        SymmetricSecurityKey signingKey
    )
    {
        var hasLifeTime = lifetimeInMinutes > 0;
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            claims: claims,
            issuer: issuer,
            audience: "*",
            notBefore: hasLifeTime ? now : null,
            expires: hasLifeTime ? now.Add(TimeSpan.FromMinutes(lifetimeInMinutes)) : null,
            signingCredentials: new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256Signature
            )
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
