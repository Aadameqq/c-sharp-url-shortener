using System.Security.Cryptography;
using Core.Domain;
using Core.Ports;
using Infrastructure.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure;

public class RedisActivationCodesRepository(
    IConnectionMultiplexer redis,
    IOptions<AccountOptions> accountOptions
) : ActivationCodesRepository
{
    public async Task<string> Create(Account account)
    {
        var code = GenerateCode();

        var db = redis.GetDatabase();
        await db.StringSetAsync(
            code,
            account.Id.ToString(),
            TimeSpan.FromMinutes(accountOptions.Value.ActivationCodeLifeSpanInMinutes)
        );

        return code;
    }

    public async Task<Guid?> GetUserIdAndRevokeCode(string code)
    {
        var db = redis.GetDatabase();
        var userId = await db.StringGetAsync(code);

        if (userId.IsNullOrEmpty)
        {
            return null;
        }

        await db.KeyDeleteAsync(code);

        return Guid.Parse(userId.ToString());
    }

    private string GenerateCode()
    {
        var rng = RandomNumberGenerator.Create();
        var randomNumber = new byte[4];

        rng.GetBytes(randomNumber);

        var generatedNumber = BitConverter.ToInt32(randomNumber, 0) & 0x7FFFFFFF;

        return (generatedNumber % 1000000).ToString("D6");
    }
}
