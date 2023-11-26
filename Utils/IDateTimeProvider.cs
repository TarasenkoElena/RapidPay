namespace RapidPay.Utils;

public interface IDateTimeProvider
{
    public DateTimeOffset GetCurrentDateTimeUtc();
}

public class DefaultDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset GetCurrentDateTimeUtc() => DateTime.UtcNow;
}
