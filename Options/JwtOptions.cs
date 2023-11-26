namespace RapidPay.Auth;

public record JwtOptions
{
    public required string SecretKey { get; init; }
    public required string ValidIssuer { get; init; }
    public required string ValidAudience { get; init; }
    public required int ExpireMinutes { get; init; }
}
