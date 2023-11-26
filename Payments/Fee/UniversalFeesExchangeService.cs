using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using RapidPay.Utils;

namespace RapidPay.Payments.Fee;

public interface IUniversalFeesExchangeService
{
    Task<decimal> GetFeeMultiplierAsync();
}

public class UniversalFeesExchangeService(IDateTimeProvider dateTimeProvider) : IUniversalFeesExchangeService
{
    private const int SecondsInHour = 3600;
    private const decimal MaxFee = 2m;

    public Task<decimal> GetFeeMultiplierAsync()
    {
        var unixTimeHours = (int)((dateTimeProvider.GetCurrentDateTimeUtc().ToUnixTimeSeconds() / SecondsInHour) % int.MaxValue);

        var fee = (decimal)new Random(unixTimeHours).NextDouble() * MaxFee;
        return Task.FromResult(fee);
    }
}

public class CachedUniversalFeesExchangeService(IMemoryCache cache, IDateTimeProvider dateTimeProvider, IUniversalFeesExchangeService ufe) : IUniversalFeesExchangeService
{
    private const string _key = "ufe_value";

    public async Task<decimal> GetFeeMultiplierAsync()
    {
        return await cache.GetOrCreateAsync(_key, async c =>
        {
            var value = await ufe.GetFeeMultiplierAsync();
            c.SetSlidingExpiration(GetExpiration());

            return value;
        });
    }

    private TimeSpan GetExpiration()
    {
        var now = dateTimeProvider.GetCurrentDateTimeUtc();
        return new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, 0, 0, now.Offset).AddHours(1) - now;
    }
}
