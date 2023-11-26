namespace RapidPay.Cards;

public interface IExternalBankCardBalanceProvider
{
    decimal? ValidateCardAndGetBalance(string number, DateOnly expirationDate, string holder);
}

public class FakeExternalBankCardBalanceProvider : IExternalBankCardBalanceProvider
{
    private readonly Random _random = new();

    public decimal? ValidateCardAndGetBalance(string number, DateOnly expirationDate, string holder)
    {
        return (decimal)_random.NextDouble() * 50_000m;
    }
}