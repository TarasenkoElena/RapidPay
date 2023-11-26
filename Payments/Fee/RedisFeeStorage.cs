using StackExchange.Redis;
using System.Globalization;

namespace RapidPay.Payments.Fee;

public interface IFeeStorage
{
    Task<decimal> MultiplyAndUpdateAsync(decimal multiplier);
}

public class RedisFeeStorage(IConnectionMultiplexer redis) : IFeeStorage
{
    private const string _key = "last_fee_value";

    const string _multiplyAndUpdateScript = @"
        local key = KEYS[1]
        local multiplier = tonumber(ARGV[1])
        local currentValue = tonumber(redis.call('GET', key) or '1.0')
        local newValue = tostring(currentValue * multiplier)
       
        redis.call('SET', key, newValue)
        return newValue
    ";

    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<decimal> MultiplyAndUpdateAsync(decimal multiplier)
    {
        var result = await _db.ScriptEvaluateAsync(_multiplyAndUpdateScript, [_key], [(double)Math.Round(multiplier, 6)]);
        return Convert.ToDecimal((string?)result, CultureInfo.InvariantCulture);
    }
}
